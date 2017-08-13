// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public class EventStoreInMemory : IEventStore
    {
        private readonly ConcurrentDictionary<SourceInfo, HashSet<EventCollection>> collection;

        public EventStoreInMemory()
        {
            collection = new ConcurrentDictionary<SourceInfo, HashSet<EventCollection>>();
        }

        public IEnumerable<IVersionedEvent> Find(SourceInfo sourceInfo, string correlationId)
        {
            HashSet<EventCollection> set;
            if (!collection.TryGetValue(sourceInfo, out set)) return null;

            return set.FirstOrDefault(item => item.CorrelationId == correlationId);
        }

        public IEnumerable<IVersionedEvent> FindAll(SourceInfo sourceInfo, int startVersion)
        {
            HashSet<EventCollection> set;
            if (!collection.TryGetValue(sourceInfo, out set)) return Enumerable.Empty<IVersionedEvent>();

            return set.SelectMany(item => item).Where(item => item.Version > startVersion).ToArray();
        }

        public bool Save(SourceInfo sourceInfo, IEnumerable<IVersionedEvent> events, string correlationId)
        {
            var set = collection.GetOrAdd(sourceInfo, () => new HashSet<EventCollection>());
            return set.Add(new EventCollection(correlationId, events));
        }

        private class EventCollection : IEnumerable<IVersionedEvent>, ICollection
        {
            private readonly List<IVersionedEvent> eventList;

            /// <summary>
            ///     Initializes a new instance of the <see cref="EventCollection" /> class.
            /// </summary>
            public EventCollection(string correlationId, IEnumerable<IVersionedEvent> events)
            {
                Assertions.NotNullOrWhiteSpace(correlationId, "correlationId");
                Assertions.NotEmpty(events, "events");

                CorrelationId = correlationId;
                eventList = new List<IVersionedEvent>(events);
            }
            

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (IEvent @event in eventList) yield return @event;
            }
            

            /// <summary>
            ///     产生事件的相关标识(如命令的id)
            /// </summary>
            public string CorrelationId { get; }

            /// <summary>
            ///     获取事件数量
            /// </summary>
            public int Count => eventList.Count;

            /// <summary>
            ///     非线程安全
            /// </summary>
            public bool IsSynchronized => false;

            /// <summary>
            ///     同步对象
            /// </summary>
            public object SyncRoot => this;

            /// <summary>
            ///     从特定的 <see cref="Array" /> 索引处开始，将 <see cref="EventCollection" /> 的元素复制到一个 <see cref="Array" /> 中。
            /// </summary>
            public void CopyTo(Array array, int index)
            {
                var destIndex = 0;
                eventList.GetRange(index, eventList.Count - index)
                    .ForEach(delegate(IEvent info) { array.SetValue(info, destIndex++); });
            }

            /// <summary>
            ///     确定此实例是否与指定的对象相同。
            /// </summary>
            public override bool Equals(object obj)
            {
                var other = obj as EventCollection;
                if (other == null) return false;

                return CorrelationId == other.CorrelationId;
            }

            /// <summary>
            ///     返回一个循环访问 <see cref="EventCollection" /> 的枚举器。
            /// </summary>
            public IEnumerator<IVersionedEvent> GetEnumerator()
            {
                return eventList.GetEnumerator();
            }

            /// <summary>
            ///     返回此实例的哈希代码。
            /// </summary>
            public override int GetHashCode()
            {
                return CorrelationId.GetHashCode();
            }
        }
    }
}