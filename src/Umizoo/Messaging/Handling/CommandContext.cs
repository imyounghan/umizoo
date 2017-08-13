// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;

using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Logging;
using Umizoo.Seeds;

namespace Umizoo.Messaging.Handling
{
    public class CommandContext : ICommandContext
    {
        private readonly Envelope<ICommand> _envelopedCommand;
        private readonly IRepository _repository;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly Dictionary<string, IAggregateRoot> _trackingAggregateRoots;

        private bool _replied;

        public CommandContext(IMessageBus<IResult> resultBus, IRepository repository, Envelope<ICommand> envelope)
        {
            _resultBus = resultBus;
            _repository = repository;
            _trackingAggregateRoots = new Dictionary<string, IAggregateRoot>();

            _envelopedCommand = envelope;
        }

        public IEnumerable<IAggregateRoot> TrackingOjbects
        {
            get { return _trackingAggregateRoots.Values; }
        }

        public ICommand Command
        {
            get { return _envelopedCommand.Body; }
        }

        public TraceInfo TraceInfo
        {
            get { return (TraceInfo) _envelopedCommand.Items.GetIfKeyNotFound(StandardMetadata.TraceInfo); }
        }

        public string CommandId
        {
            get { return _envelopedCommand.MessageId; }
        }

        public void Add(IAggregateRoot aggregateRoot)
        {
            Assertions.NotNull(aggregateRoot, "aggregateRoot");

            var key = string.Concat(aggregateRoot.GetType().FullName, "@", aggregateRoot.Id);
            _trackingAggregateRoots.TryAdd(key, aggregateRoot);
        }

        public TEventSourced Get<TEventSourced, TIdentify>(TIdentify id) where TEventSourced : class, IEventSourced
        {
            var aggregateRoot = Find<TEventSourced, TIdentify>(id);
            if (aggregateRoot == null)
                throw new EntityNotFoundException(id, typeof(TEventSourced));

            return aggregateRoot;
        }

        public TAggregateRoot Find<TAggregateRoot, TIdentify>(TIdentify id) where TAggregateRoot : class, IAggregateRoot
        {
            var aggregateRootType = typeof(TAggregateRoot);
            if (!aggregateRootType.IsClass || aggregateRootType.IsAbstract)
            {
                var errorMessage = string.Format("type of '{0}' must be a non abstract class.",
                    aggregateRootType.FullName);
                throw new ApplicationException(errorMessage);
            }

            var key = string.Concat(aggregateRootType.FullName, "@", id);

            IAggregateRoot aggregateRoot;
            if (!_trackingAggregateRoots.TryGetValue(key, out aggregateRoot))
            {
                aggregateRoot = _repository.Find(aggregateRootType, id);

                if (!aggregateRoot.IsNull()) _trackingAggregateRoots.Add(key, aggregateRoot);
            }

            return aggregateRoot as TAggregateRoot;
        }

        public void Complete(object result, Func<object, string> serializer)
        {
            if (_replied) return;


            var commandResult = CommandResult.Finished;
            if (!result.IsNull())
                if (!serializer.IsNull())
                    commandResult = new CommandResult
                    {
                        Result = serializer(result),
                        ReplyType = CommandReturnMode.Manual
                    };
            _resultBus.Send(commandResult, TraceInfo);

            _replied = true;
        }


        public void Commit()
        {
            var dirtyAggregateRootCount = 0;
            var dirtyAggregateRoot = default(IEventSourced);
            var changedEvents = Enumerable.Empty<IVersionedEvent>();
            foreach (var aggregateRoot in _trackingAggregateRoots.Values.OfType<IEventSourced>())
            {
                changedEvents = aggregateRoot.GetChanges();
                if (!changedEvents.IsEmpty())
                {
                    dirtyAggregateRootCount++;
                    dirtyAggregateRoot = aggregateRoot;
                }
            }

            CommandResult commandResult;
            if (dirtyAggregateRootCount == 0)
            {
                LogManager.Default.ErrorFormat(
                    "not found aggregateroot to be created or modified by command. commandType:{0}, commandId:{1}",
                    Command.GetType().FullName,
                    CommandId);

                commandResult = new CommandResult(HandleStatus.Nothing,
                    "not found aggregateroot to be created or modified.");
            }
            else if (dirtyAggregateRootCount > 1)
            {
                LogManager.Default.ErrorFormat(
                    "Detected more than one aggregate created or modified by command. commandType:{0}, commandId:{1}",
                    Command.GetType().FullName,
                    CommandId);

                commandResult = new CommandResult(HandleStatus.Failed,
                    "Detected more than one aggregate created or modified.");
            }
            else
            {
                _repository.Save(dirtyAggregateRoot, _envelopedCommand);
                commandResult = new CommandResult
                {
                    ReplyType = CommandReturnMode.CommandExecuted,
                    ProducedEventCount = changedEvents.Count()
                };
            }

            if (_replied) return;
            _resultBus.Send(commandResult, TraceInfo);
        }
    }
}