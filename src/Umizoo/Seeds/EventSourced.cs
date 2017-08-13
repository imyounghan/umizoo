// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    /// <summary>
    ///     <see cref="IEventSourced" /> 的抽象实现类
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class EventSourced<TIdentify> : AggregateRoot<TIdentify>, IEventSourced
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        protected EventSourced()
        {
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        protected EventSourced(TIdentify id)
            : base(id)
        {
        }

        /// <summary>
        ///     版本号
        /// </summary>
        [DataMember(Name = "version")]
        public int Version { get; protected internal set; }

        protected new void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : VersionedEvent
        {
            @event.Version = Version + 1;
            if (!base.RaiseEvent(@event))
            {
                throw new ApplicationException(string.Format("Event Handler not found on '{0}' for '{1}'.", GetType().FullName, @event.GetType().FullName));
            }
            Version = @event.Version;
        }

        #region IEventSourced 成员

        void IEventSourced.LoadFrom(IEnumerable<IVersionedEvent> events)
        {
            foreach (var @event in events)
            {
                if (@event.Version != Version + 1)
                {
                    var errorMessage = string.Format(
                        "Cannot load because the version '{0}' is not equal to the AggregateRoot version '{1}' on '{2}' of id '{3}'.",
                        @event.Version, Version, GetType().FullName, Id);
                    throw new ApplicationException(errorMessage);
                }

                base.RaiseEvent(@event);
                Version = @event.Version;
            }

            ClearEvents();
        }

        IEnumerable<IVersionedEvent> IEventSourced.GetChanges()
        {
            return GetEvents().OfType<IVersionedEvent>().ToArray();
        }

        //void IEventSourced.AcceptChanges(int newVersion)
        //{
        //    if(this.Version + 1 != newVersion)
        //    {
        //        var errorMessage =
        //            string.Format(
        //                "Cannot accept invalid version: {0}, expect version: {1}, current aggregateRoot type: {2}, id: {3}",
        //                newVersion,
        //                this.Version + 1,
        //                this.GetType().FullName,
        //                this.Id);
        //        throw new ApplicationException(errorMessage);
        //    }
        //    this.Version = newVersion;
        //    this.ClearEvents();
        //}

        #endregion
    }
}