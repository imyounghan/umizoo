// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using System;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public class EventConsumer : MessageConsumer<IEvent>, IInitializer
    {
        private readonly IMessageBus<ICommand> _commandBus;
        private readonly IMessageBus<IEvent> _eventBus;

        private readonly Dictionary<Type, IEventHandler> _eventHandlers;
        private readonly IEventPublishedVersionStore _publishedVersionStore;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly ITextSerializer _serializer;


        /// <summary>
        ///     Initializes a new instance of the <see cref="EventConsumer" /> class.
        /// </summary>
        public EventConsumer(IMessageBus<ICommand> commandBus,
            IMessageBus<IEvent> eventBus,
            IMessageBus<IResult> resultBus,
            IEventPublishedVersionStore publishedVersionStore,
            ITextSerializer serializer,
            IMessageReceiver<Envelope<IEvent>> eventReceiver)
            : base(eventReceiver, ProcessingFlags.Event)
        {
            _eventHandlers = new Dictionary<Type, IEventHandler>();

            _publishedVersionStore = publishedVersionStore;
            _resultBus = resultBus;
            _commandBus = commandBus;
            _eventBus = eventBus;
            _serializer = serializer;
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            var versionedEventType = typeof(IVersionedEvent);
            types.Where(type => type.IsClass && !type.IsAbstract && typeof(IEvent).IsAssignableFrom(type))
                .ForEach(eventType =>
                {
                    if (versionedEventType.IsAssignableFrom(eventType))
                    {
                        var eventHandlers =
                            container.ResolveAll(typeof(IEventHandler<>).MakeGenericType(eventType))
                                .OfType<IEventHandler>()
                                .ToList();
                        switch (eventHandlers.Count)
                        {
                            case 0:
                                break;
                            case 1:
                                _eventHandlers[eventType] = eventHandlers.First();
                                break;
                            default:
                                throw new SystemException(string.Format(
                                    "Found more than one handler for '{0}' with IEventHandler<>.",
                                    eventType.FullName));
                        }
                    }

                    Initialize(container, eventType);
                });
        }


        private bool ProcessEvent(IEventHandler eventHandler, IVersionedEvent @event, string eventId,
            IDictionary<string, object> items)
        {
            var sourceInfo = (SourceInfo) items[StandardMetadata.SourceInfo];

            if (@event.Version > 1)
            {
                var lastPublishedVersion = _publishedVersionStore.GetPublishedVersion(sourceInfo) + 1;
                if (lastPublishedVersion < @event.Version)
                {
                    var envelope = new Envelope<IEvent>(@event, eventId)
                    {
                        Items = items
                    };
                    _eventBus.Send(envelope);

                    if (LogManager.Default.IsDebugEnabled)
                        LogManager.Default.DebugFormat(
                            "The event cannot be process now as the version is not the next version, it will be handle later. aggregateRootType={0},aggregateRootId={1},lastPublishedVersion={2},eventVersion={3},eventType={4}.",
                            sourceInfo.Type.FullName,
                            sourceInfo.Id,
                            lastPublishedVersion,
                            @event.Version,
                            @event.GetType().FullName);

                    return false;
                }

                if (lastPublishedVersion > @event.Version)
                {
                    if (LogManager.Default.IsDebugEnabled)
                        LogManager.Default.DebugFormat(
                            "The event is ignored because it is obsoleted. aggregateRootType={0},aggregateRootId={1},lastPublishedVersion={2},eventVersion={3},eventType={4}.",
                            sourceInfo.Type.FullName,
                            sourceInfo.Id,
                            lastPublishedVersion,
                            @event.Version,
                            @event.GetType().FullName);

                    return false;
                }
            }

            try
            {
                var envelope = new Envelope<IVersionedEvent>(@event, eventId)
                {
                    Items = items
                };
                TryMultipleInvoke(InvokeHandler, eventHandler, envelope);
            }
            catch (Exception ex)
            {
                var commandResult = new CommandResult(HandleStatus.SyncFailed, ex.Message)
                {
                    Result = _serializer.Serialize(ex.Data),
                    ReplyType = CommandReturnMode.EventHandled
                };
                var traceInfo = (TraceInfo) items[StandardMetadata.TraceInfo];
                _resultBus.Send(commandResult, traceInfo);
            }

            _publishedVersionStore.AddOrUpdatePublishedVersion(sourceInfo, @event.Version);

            return true;
        }

        private void InvokeHandler(IEventHandler eventHandler, Envelope<IVersionedEvent> envelope)
        {
            var context = new EventContext(_commandBus, _resultBus);
            if (envelope.Items.ContainsKey(StandardMetadata.SourceInfo))
                context.SourceInfo = (SourceInfo) envelope.Items[StandardMetadata.SourceInfo];
            if (envelope.Items.ContainsKey(StandardMetadata.TraceInfo))
                context.TraceInfo = (TraceInfo) envelope.Items[StandardMetadata.TraceInfo];
            if (envelope.Items.ContainsKey(StandardMetadata.CommandInfo))
                context.CommandInfo = (SourceInfo) envelope.Items[StandardMetadata.CommandInfo];

            ((dynamic) eventHandler).Handle((dynamic) context, (dynamic) envelope.Body);
            context.Commit();
        }

        protected override void OnMessageReceived(Envelope<IEvent> envelope)
        {
            var eventType = envelope.Body.GetType();
            IEventHandler handler;
            if (_eventHandlers.TryGetValue(eventType, out handler))
                if (!ProcessEvent(handler, (IVersionedEvent) envelope.Body, envelope.MessageId, envelope.Items)) return;

            base.OnMessageReceived(envelope);
        }
    }
}