
namespace Umizoo.Messaging
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Umizoo.Infrastructure;

    public class EventStoreInMemory : IEventStore
    {
        class EventCollection : IEnumerable<IVersionedEvent>, ICollection
        {
            #region Fields

            private readonly List<IVersionedEvent> eventList;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="EventCollection"/> class. 
            /// </summary>
            public EventCollection(string correlationId, IEnumerable<IVersionedEvent> events)
            {
                Ensure.NotNullOrWhiteSpace(correlationId, "correlationId");
                Ensure.NotEmpty(events, "events");

                this.CorrelationId = correlationId;
                this.eventList = new List<IVersionedEvent>(events);
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     产生事件的相关标识(如命令的id)
            /// </summary>
            public string CorrelationId { get; private set; }
            
            /// <summary>
            ///     获取事件数量
            /// </summary>
            public int Count
            {
                get
                {
                    return this.eventList.Count;
                }
            }

            /// <summary>
            ///     非线程安全
            /// </summary>
            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            ///     同步对象
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    return this;
                }
            }

            #endregion


            #region Methods and Operators

            /// <summary>
            /// 从特定的 <see cref="Array"/> 索引处开始，将 <see cref="EventCollection"/> 的元素复制到一个 <see cref="Array"/> 中。
            /// </summary>
            public void CopyTo(Array array, int index)
            {
                int destIndex = 0;
                this.eventList.GetRange(index, this.eventList.Count - index)
                    .ForEach(delegate(IEvent info) { array.SetValue(info, destIndex++); });
            }

            /// <summary>
            /// 确定此实例是否与指定的对象相同。
            /// </summary>
            public override bool Equals(object obj)
            {
                var other = obj as EventCollection;
                if(other == null) {
                    return false;
                }

                return this.CorrelationId == other.CorrelationId;
            }

            /// <summary>
            /// 返回一个循环访问 <see cref="EventCollection"/> 的枚举器。
            /// </summary>
            public IEnumerator<IVersionedEvent> GetEnumerator()
            {
                return this.eventList.GetEnumerator();
            }

            /// <summary>
            /// 返回此实例的哈希代码。
            /// </summary>
            public override int GetHashCode()
            {
                return this.CorrelationId.GetHashCode();
            }

            #endregion

            #region Explicit Interface Methods
            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach(IEvent @event in this.eventList) {
                    yield return @event;
                }
            }

            #endregion
        }


        private readonly ConcurrentDictionary<SourceInfo, HashSet<EventCollection>> collection;

        public EventStoreInMemory()
        {
            this.collection = new ConcurrentDictionary<SourceInfo, HashSet<EventCollection>>();
        }

        public IEnumerable<IVersionedEvent> Find(SourceInfo sourceInfo, string correlationId)
        {
            HashSet<EventCollection> set;
            if (!collection.TryGetValue(sourceInfo, out set)) {
                return null;
            }

            return set.FirstOrDefault(item => item.CorrelationId == correlationId);
        }

        public IEnumerable<IVersionedEvent> FindAll(SourceInfo sourceInfo, int startVersion)
        {
            HashSet<EventCollection> set;
            if (!collection.TryGetValue(sourceInfo, out set)) {
                return Enumerable.Empty<IVersionedEvent>();
            }

            return set.SelectMany(item => item).Where(item => item.Version > startVersion).ToArray();
        }

        public bool Save(SourceInfo sourceInfo, IEnumerable<IVersionedEvent> events, string correlationId)
        {
            var set = collection.GetOrAdd(sourceInfo, () => new HashSet<EventCollection>());
            return set.Add(new EventCollection(correlationId, events));
        }
    }
}
