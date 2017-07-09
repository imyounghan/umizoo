

namespace Umizoo.Seeds
{
    using System;
    using System.Linq;

    using Umizoo.Infrastructure;
    using Umizoo.Messaging;

    public class MemoryRepository : IRepository
    {
        private readonly IMessageBus<IEvent> _eventBus;
        private readonly ICache _cache;

        public MemoryRepository(IMessageBus<IEvent> eventBus, ICache cache)
        {
            this._eventBus = eventBus;
            this._cache = cache;
        }

        private static bool IsAggregateRoot(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(IAggregateRoot).IsAssignableFrom(type);
        }
        #region IRepository 成员

        public IAggregateRoot Find(Type aggregateRootType, object aggregateRootId)
        {
            if(!IsAggregateRoot(aggregateRootType)) {
                string errorMessage = string.Format("The type of '{0}' does not extend interface IAggregateRoot.", aggregateRootType.FullName);
                throw new ApplicationException(errorMessage);
            }

            IAggregateRoot aggregateRoot;
            if(_cache.TryGet(aggregateRootType, aggregateRootId, out aggregateRoot)) {
                //if(_logger.IsDebugEnabled)
                //    _logger.DebugFormat("find the aggregate root '{0}' of id '{1}' from cache.",
                //        aggregateRootType.FullName, aggregateRootId);

                return aggregateRoot;
            }

            return null;
        }

        public void Save(IAggregateRoot aggregateRoot)
        {
            _cache.Set(aggregateRoot, aggregateRoot.Id);

            var eventPublisher = aggregateRoot as IEventPublisher;
            if(eventPublisher == null) {
                return;
            }

            var sourceInfo = new SourceInfo(aggregateRoot.Id, aggregateRoot.GetType());
            var envelopes = eventPublisher.GetEvents().Select(@event => {
                            var envelope = new Envelope<IEvent>(@event, ObjectId.GenerateNewStringId());
                            envelope.Items[StandardMetadata.SourceInfo] = sourceInfo;
                            return envelope;
                        }).ToArray();
                

            _eventBus.Send(envelopes);
        }

        public void Delete(IAggregateRoot aggregateRoot)
        {
            var aggregateRootType = aggregateRoot.GetType();
            _cache.Remove(aggregateRootType, aggregateRoot.Id);
        }

        #endregion
    }
}
