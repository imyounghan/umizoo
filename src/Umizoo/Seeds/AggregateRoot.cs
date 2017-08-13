// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    /// <summary>
    ///     <see cref="IAggregateRoot" /> 的抽象实现类
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class AggregateRoot<TIdentify> : Entity<TIdentify>, IAggregateRoot, IEventPublisher
    {
        [IgnoreDataMember] [NonSerialized] private List<IEvent> _pendingEvents;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        protected AggregateRoot()
        {
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        protected AggregateRoot(TIdentify id)
            : base(id)
        {
        }


        #region IEventPublisher 成员

        IEnumerable<IEvent> IEventPublisher.GetEvents()
        {
            if (_pendingEvents == null || _pendingEvents.Count == 0) return Enumerable.Empty<IEvent>();
            return new ReadOnlyCollection<IEvent>(_pendingEvents);
        }

        #endregion

        /// <summary>
        ///     引发事件并将其加入到待发布事件列表
        /// </summary>
        /// <param name="event">事件</param>
        /// <returns>如果聚合内部有事件对应的处理器则返回true，否则为false</returns>
        protected bool RaiseEvent<TEvent>(TEvent @event)
            where TEvent : class, IEvent
        {
            if (_pendingEvents == null) _pendingEvents = new List<IEvent>();
            _pendingEvents.Add(@event);

            return ApplyEvent(@event);
        }

        private bool ApplyEvent(IEvent @event)
        {
            var eventType = @event.GetType();
            var aggregateRootType = GetType();

            Action<IAggregateRoot, IEvent> innerHandler;
            if (AggregateInnerHandlerProvider.Current.TryGetHandler(aggregateRootType, eventType, out innerHandler))
            {
                innerHandler.Invoke(this, @event);
                return true;
            }

            return false;
        }


        protected void ClearEvents()
        {
            if (_pendingEvents == null || _pendingEvents.Count == 0) return;

            _pendingEvents.Clear();
        }

        protected IEnumerable<IEvent> GetEvents()
        {
            return _pendingEvents;
        }
    }
}