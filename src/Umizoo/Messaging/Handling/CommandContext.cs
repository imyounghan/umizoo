

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Umizoo.Infrastructure;
    using Umizoo.Seeds;

    public class CommandContext : ICommandContext
    {
        private readonly Dictionary<string, IAggregateRoot> trackingAggregateRoots;

        private readonly IEventStore eventStore;
        private readonly ICache cache;
        private readonly ISnapshotStore snapshotStore;
        private readonly IMessageBus<IEvent> eventBus;
        private readonly IMessageBus<IResult> resultBus;

        private readonly IRepository repository;

        private bool replied;

        public CommandContext(IMessageBus<IEvent> eventBus,
            IMessageBus<IResult> resultBus,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            IRepository repository,
            ICache cache)
        {
            this.eventBus = eventBus;
            this.resultBus = resultBus;
            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.repository = repository;
            this.cache = cache;

            this.trackingAggregateRoots = new Dictionary<string, IAggregateRoot>();
        }

        public IEnumerable<IAggregateRoot> TrackingOjbects
        {
            get
            {
                return trackingAggregateRoots.Values;
            }
        }

        public string CommandId { get; set; }

        public ICommand Command { get; set; }

        public TraceInfo TraceInfo { get; set; }

        public void Add(IAggregateRoot aggregateRoot)
        {
            Ensure.NotNull(aggregateRoot, "aggregateRoot");

            string key = string.Concat(aggregateRoot.GetType().FullName, "@", aggregateRoot.Id);
            trackingAggregateRoots.TryAdd(key, aggregateRoot);
        }

        public TEventSourced Get<TEventSourced, TIdentify>(TIdentify id) where TEventSourced : class, IEventSourced
        {
            var aggregateRoot = this.Find<TEventSourced, TIdentify>(id);
            if (aggregateRoot == null)
                throw new EntityNotFoundException(id, typeof(TEventSourced));

            return aggregateRoot;
        }

        public TAggregateRoot Find<TAggregateRoot, TIdentify>(TIdentify id) where TAggregateRoot : class, IAggregateRoot
        {
            var type = typeof(TAggregateRoot);
            if (!type.IsClass || type.IsAbstract) {
                string errorMessage = string.Format("The type of '{0}' must be a non abstract class.", type.FullName);
                throw new ApplicationException(errorMessage);
            }

            string key = string.Concat(type.FullName, "@", id);

            IAggregateRoot aggregateRoot;
            if (!trackingAggregateRoots.TryGetValue(key, out aggregateRoot)) {
                if (typeof(IEventSourced).IsAssignableFrom(type)) {
                    aggregateRoot = this.Restore(type, id);
                }
                else {
                    aggregateRoot = this.repository.Find(type, id);
                }

                if (aggregateRoot != null) {
                    trackingAggregateRoots.Add(key, aggregateRoot);
                }
            }

            return aggregateRoot as TAggregateRoot;
        }

        private IEventSourced Create(Type sourceType, object sourceId)
        {
            var idType = sourceId.GetType();
            var constructor = sourceType.GetConstructor(new[] { idType });

            if (constructor == null) {
                string errorMessage = string.Format("Type '{0}' must have a constructor with the following signature: .ctor({1} id)", sourceType.FullName, idType.FullName);
                throw new ApplicationException(errorMessage);
                //return FormatterServices.GetUninitializedObject(aggregateRootType) as IEventSourced;
            }

            return constructor.Invoke(new[] { sourceId }) as IEventSourced;
        }

        /// <summary>
        /// 根据主键获取聚合根实例。
        /// </summary>
        private IEventSourced Restore(Type sourceType, object sourceId)
        {
            Ensure.NotNull(sourceId, "sourceId");
            Ensure.NotNull(sourceType, "sourceType");

            IEventSourced aggregateRoot = null;
            if (this.cache.TryGet(sourceType, sourceId, out aggregateRoot)) {
                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat("Find the aggregate root {0} of id {1} from cache.",
                        sourceType.FullName, sourceId);
                }

                return aggregateRoot;
            }

            try {
                aggregateRoot = this.snapshotStore.GetLastest(sourceType, sourceId) as IEventSourced;
                if (aggregateRoot != null) {
                    if (LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.DebugFormat("Find the aggregate root '{0}' of id '{1}' from snapshot. current version:{2}.",
                            sourceType.FullName, sourceId, aggregateRoot.Version);
                    }
                }
            }
            catch (Exception ex) {
                LogManager.Default.Warn(ex,
                    "Get the latest snapshot failed. aggregateRootId:{0},aggregateRootType:{1}.",
                    sourceId, sourceType.FullName);
            }


            var events = this.eventStore.FindAll(new SourceInfo(sourceId, sourceType), aggregateRoot.Version);
            if (!events.IsEmpty()) {
                if (aggregateRoot == null) {
                    aggregateRoot = this.Create(sourceType, sourceId);
                }

                aggregateRoot.LoadFrom(events);
                //foreach(var @event in events) {
                //    aggregateRoot.LoadFrom(@event);
                //    aggregateRoot.AcceptChanges(@event.Version);
                //}

                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat("Restore the aggregate root '{0}' of id '{1}' from event stream. version:{2} ~ {3}",
                        sourceType.FullName,
                        sourceId,
                        events.Min(p => p.Version),
                        events.Max(p => p.Version));
                }
            }

            if (aggregateRoot != null) {
                this.cache.Set(aggregateRoot, sourceId);
            }

            return aggregateRoot;
        }

        public void Commit()
        {
            var dirtyAggregateRootCount = 0;
            var dirtyAggregateRoot = default(IEventSourced);
            var changedEvents = Enumerable.Empty<IVersionedEvent>();
            foreach (var aggregateRoot in trackingAggregateRoots.Values.OfType<IEventSourced>()) {
                changedEvents = aggregateRoot.GetChanges();
                if (!changedEvents.IsEmpty()) {
                    dirtyAggregateRootCount++;
                    if (dirtyAggregateRootCount > 1) {
                        var errorMessage =
                            string.Format(
                                "Detected more than one aggregate created or modified by command. commandType:{0}, commandId:{1}",
                                this.Command.GetType().FullName,
                                this.CommandId);
                        throw new UnrecoverableException(errorMessage);
                    }
                    dirtyAggregateRoot = aggregateRoot;
                }
            }

            if (dirtyAggregateRootCount == 0 || changedEvents.IsEmpty()) {
                var errorMessage = string.Format("Not found aggregate to be created or modified by command. commandType:{0}, commandId:{1}",
                    this.Command.GetType().FullName,
                    this.CommandId);
                throw new UnrecoverableException(errorMessage);
            }

            var aggregateRootType = dirtyAggregateRoot.GetType();

            var sourceInfo = new SourceInfo(dirtyAggregateRoot.Id, aggregateRootType);

            var envelopedCommand = new Envelope<ICommand>(this.Command, this.CommandId);
            envelopedCommand.Items[StandardMetadata.TraceInfo] = this.TraceInfo;
            try {
                if (this.eventStore.Save(sourceInfo, changedEvents, this.CommandId)) {
                    if (LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.DebugFormat("Persistent domain events successfully. aggregateRootType:{0},aggregateRootId:{1},version:{2}.",
                            aggregateRootType.FullName, dirtyAggregateRoot.Id, dirtyAggregateRoot.Version);
                    }
                }
                else {
                    changedEvents = this.eventStore.Find(sourceInfo, this.CommandId);

                    this.eventBus.Publish(sourceInfo, changedEvents, envelopedCommand);
                    return;
                }
            }
            catch (Exception ex) {
                LogManager.Default.Error(ex,
                        "Persistent domain events failed. aggregateRootType:{0},aggregateRootId:{1},version:{2}.",
                        dirtyAggregateRoot.Id, aggregateRootType.FullName, dirtyAggregateRoot.Version);
                throw ex;
            }


            try {
                this.cache.Set(dirtyAggregateRoot, dirtyAggregateRoot.Id);
            }
            catch (Exception ex) {
                LogManager.Default.Warn(ex,
                        "Failed to refresh aggregate root to memory cache. aggregateRootType:{0},aggregateRootId:{1},commandId:{2}.",
                        dirtyAggregateRoot.Id, aggregateRootType.FullName, this.CommandId);
            }


            this.eventBus.Publish(sourceInfo, changedEvents, envelopedCommand);

            if (replied) {
                return;
            }
            var commandResult = new CommandResult() {
                ReplyType = CommandReturnMode.CommandExecuted,
                ProduceEventCount = changedEvents.Count()
            };
            resultBus.Send(commandResult, this.TraceInfo);
        }

        public void Complete(object result, Func<object, string> serializer)
        {
            if (replied) {
                return;
            }

            this.replied = true;

            if (result == null || result == DBNull.Value) {
                resultBus.Send(CommandResult.ManualCompleted, this.TraceInfo);
                return;
            }

            Ensure.NotNull(serializer, "serializer");
            var commandResult = new CommandResult() {
                Result = serializer(result),
                ReplyType = CommandReturnMode.Manual
            };
            resultBus.Send(commandResult, this.TraceInfo);
        }
    }
}
