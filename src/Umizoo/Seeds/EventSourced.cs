

namespace Umizoo.Seeds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Umizoo.Messaging;
    using Umizoo.Messaging.Handling;

    /// <summary>
    /// <see cref="IEventSourced"/> 的抽象实现类
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class EventSourced<TIdentify> : AggregateRoot<TIdentify>, IEventSourced
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected EventSourced()
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected EventSourced(TIdentify id)
            : base(id)
        { }

        /// <summary>
        /// 版本号
        /// </summary>
        [DataMember(Name = "version")]
        public int Version { get; protected internal set; }

        new protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : VersionedEvent
        {
            @event.Version = this.Version + 1;
            if (!base.RaiseEvent(@event))
            {
                throw new HandlerNotFoundException(this.GetType(), @event.GetType());  
            }
            this.Version = @event.Version;
            
        }
        #region IEventSourced 成员


        void IEventSourced.LoadFrom(IEnumerable<IVersionedEvent> events)
        {
            foreach(var @event in events) {
                if(@event.Version != this.Version + 1) {
                    var errorMessage = string.Format("Cannot load because the version '{0}' is not equal to the AggregateRoot version '{1}' on '{2}' of id '{3}'.",
                        @event.Version, this.Version, this.GetType().FullName, this.Id);
                    throw new ApplicationException(errorMessage);
                }

                base.RaiseEvent(@event);
                this.Version = @event.Version;
            }

            this.ClearEvents();
        }

        IEnumerable<IVersionedEvent> IEventSourced.GetChanges()
        {
            return this.GetEvents().OfType<IVersionedEvent>().ToArray();
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
