
namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Composition;

    public class EventConsumer : MessageConsumer<IEvent>//, IInitializer
    {
        #region Fields

        private readonly ConcurrentDictionary<Type, IEventHandler> _eventHandlers;
        private readonly IMessageBus<ICommand> _commandBus;
        private readonly IMessageBus<IEvent> _eventBus;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly IEventPublishedVersionStore _publishedVersionStore;
        private readonly ITextSerializer _serializer;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EventConsumer"/> class.
        /// </summary>
        public EventConsumer(IMessageBus<ICommand> commandBus,
            IMessageBus<IEvent> eventBus,
            IMessageBus<IResult> resultBus,
            IEventPublishedVersionStore publishedVersionStore,
            ITextSerializer serializer,
            IMessageReceiver<Envelope<IEvent>> eventReceiver)
            : base(eventReceiver)
        {
            this._eventHandlers = new ConcurrentDictionary<Type, IEventHandler>();

            this._publishedVersionStore = publishedVersionStore;
            this._resultBus = resultBus;
            this._commandBus = commandBus;
            this._eventBus = eventBus;
            this._serializer = serializer;
        }

        private bool ProcessEvent(IEventHandler eventHandler, IVersionedEvent @event, string eventId, IDictionary<string, object> items)
        {
            var sourceInfo = (SourceInfo)items[StandardMetadata.SourceInfo];

            if (@event.Version > 1) {
                int lastPublishedVersion = this._publishedVersionStore.GetPublishedVersion(sourceInfo) + 1;
                if (lastPublishedVersion < @event.Version) {
                    var envelope = new Envelope<IEvent>(@event, eventId) {
                        Items = items
                    };
                    _eventBus.Send(envelope);

                    if (LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.DebugFormat(
                            "The event cannot be process now as the version is not the next version, it will be handle later. aggregateRootType={0},aggregateRootId={1},lastPublishedVersion={2},eventVersion={3},eventType={4}.",
                            sourceInfo.Type.FullName,
                            sourceInfo.Id,
                            lastPublishedVersion,
                            @event.Version,
                            @event.GetType().FullName);
                    }

                    return false;
                }

                if (lastPublishedVersion > @event.Version) {
                    if (LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.DebugFormat(
                            "The event is ignored because it is obsoleted. aggregateRootType={0},aggregateRootId={1},lastPublishedVersion={2},eventVersion={3},eventType={4}.",
                             sourceInfo.Type.FullName,
                            sourceInfo.Id,
                            lastPublishedVersion,
                            @event.Version,
                            @event.GetType().FullName);
                    }

                    return false;
                }
            }

            try {
                var envelope = new Envelope<IVersionedEvent>(@event, eventId) {
                    Items = items
                };
                this.TryMultipleInvoke(this.InvokeHandler, eventHandler, envelope);
            }
            catch (Exception ex) {
                var commandResult = new CommandResult(HandleStatus.SyncFailed, ex.Message) {
                    Result = _serializer.Serialize(ex.Data),
                    ReplyType = CommandReturnMode.EventHandled
                };
                var traceInfo = (TraceInfo)items[StandardMetadata.TraceInfo];
                this._resultBus.Send(commandResult, traceInfo);
            }

            this._publishedVersionStore.AddOrUpdatePublishedVersion(sourceInfo, @event.Version);

            return true;
        }

        private void InvokeHandler(IEventHandler eventHandler, Envelope<IVersionedEvent> envelope)
        {
            var context = new EventContext(this._commandBus, this._resultBus);
            if (envelope.Items.ContainsKey(StandardMetadata.SourceInfo)) {
                context.SourceInfo = (SourceInfo)envelope.Items[StandardMetadata.SourceInfo];
            }
            if (envelope.Items.ContainsKey(StandardMetadata.TraceInfo)) {
                context.TraceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];
            }
            if (envelope.Items.ContainsKey(StandardMetadata.CommandInfo)) {
                context.CommandInfo = (SourceInfo)envelope.Items[StandardMetadata.CommandInfo];
            }

            ((dynamic)eventHandler).Handle((dynamic)context, (dynamic)envelope.Body);
            context.Commit();
        }

        protected override void ProcessMessage(Envelope<IEvent> envelope, Type eventType)
        {
            IEventHandler handler;
            if (this._eventHandlers.TryGetValue(eventType, out handler)) {
                if (!this.ProcessEvent(handler, (IVersionedEvent)envelope.Body, envelope.MessageId, envelope.Items)) {
                    return;
                }
            }

            base.ProcessMessage(envelope, eventType);
        }

        public override void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies)
        {
            Type versionedType = typeof(IVersionedEvent);

            foreach (var eventType in Configuration.Current.EventTypes.Values) {

                if (versionedType.IsAssignableFrom(eventType)) {
                    List<IEventHandler> eventHandlers =
                    container.ResolveAll(typeof(IEventHandler<>).MakeGenericType(eventType))
                        .OfType<IEventHandler>()
                        .ToList();
                    switch (eventHandlers.Count) {
                        case 0:
                            break;
                        case 1:
                            this._eventHandlers[eventType] = eventHandlers.First();
                            break;
                        default:
                            throw new SystemException(string.Format(
                                    "Found more than one handler for '{0}' with IEventHandler<>.",
                                    eventType.FullName));
                    }
                }

                this.Initialize(container, eventType);
            }
        }
    }
}