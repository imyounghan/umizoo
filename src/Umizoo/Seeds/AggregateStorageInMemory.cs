// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    public class AggregateStorageInMemory : IAggregateStorage
    {
        private readonly ConcurrentDictionary<Type, HashSet<IAggregateRoot>> _cache;
        private readonly IMessageBus<IEvent> _eventBus;

        public AggregateStorageInMemory(IMessageBus<IEvent> eventBus)
        {
            this._cache = new ConcurrentDictionary<Type, HashSet<IAggregateRoot>>();
            this._eventBus = eventBus;
        }

        public void Delete(IAggregateRoot aggregateRoot)
        {
            HashSet<IAggregateRoot> set;
            if (!_cache.TryGetValue(aggregateRoot.GetType(), out set)) {
                return;
            }
            set.RemoveWhere(p => p.Id.GetHashCode() == aggregateRoot.Id.GetHashCode());
        }

        public IAggregateRoot Find(Type aggregateRootType, object aggregateRootId)
        {
            if (!aggregateRootType.IsAssignableFrom(typeof(IAggregateRoot))) {
                string errorMessage = string.Format("The type of '{0}' does not extend interface IAggregateRoot.", aggregateRootType.FullName);
                throw new ApplicationException(errorMessage);
            }

            HashSet<IAggregateRoot> set;
            if (!_cache.TryGetValue(aggregateRootType, out set)) {
                return null;
            }

            return set.Where(p => p.Id.GetHashCode() == aggregateRootId.GetHashCode()).FirstOrDefault();
        }

        public void Save(IAggregateRoot aggregateRoot)
        {
            var set = _cache.GetOrAdd(aggregateRoot.GetType(), () => new HashSet<IAggregateRoot>());
            if (!set.Add(aggregateRoot))
                return;

            var eventPublisher = aggregateRoot as IEventPublisher;
            if (eventPublisher == null)
                return;

            var events = eventPublisher.GetEvents();

            if (events.IsEmpty())
                return;

            _eventBus.Publish(events);
        }
    }
}