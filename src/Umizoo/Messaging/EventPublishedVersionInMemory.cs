namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Concurrent;
    

    public class EventPublishedVersionInMemory : IEventPublishedVersionStore
    {
        private readonly ConcurrentDictionary<SourceInfo, int>[] _versionCaches;

        public EventPublishedVersionInMemory()
            : this(5)
        { }

        protected EventPublishedVersionInMemory(int dictCount)
        {
            this._versionCaches = new ConcurrentDictionary<SourceInfo, int>[dictCount];
            for (int index = 0; index < dictCount; index++) {
                _versionCaches[index] = new ConcurrentDictionary<SourceInfo, int>();
            }
        }

        public virtual void AddOrUpdatePublishedVersion(SourceInfo sourceInfo, int version)
        { }

        public virtual int GetPublishedVersion(SourceInfo sourceInfo)
        {
            return 0;
        }

        private int GetPublishedVersionFromMemory(SourceInfo sourceKey)
        {
            var dict = _versionCaches[Math.Abs(sourceKey.GetHashCode() % _versionCaches.Length)];
            int version;
            if (dict.TryGetValue(sourceKey, out version)) {
                return version;
            }

            return -1;
        }

        private void AddOrUpdatePublishedVersionToMemory(SourceInfo sourceKey, int version)
        {
            var dict = _versionCaches[Math.Abs(sourceKey.GetHashCode() % _versionCaches.Length)];

            dict.AddOrUpdate(sourceKey,
                version,
                (key, value) => version == value + 1 ? version : value);
        }

        void IEventPublishedVersionStore.AddOrUpdatePublishedVersion(SourceInfo sourceInfo, int version)
        {
            this.AddOrUpdatePublishedVersionToMemory(sourceInfo, version);
            this.AddOrUpdatePublishedVersion(sourceInfo, version);
        }

        int IEventPublishedVersionStore.GetPublishedVersion(SourceInfo sourceInfo)
        {
            var version = this.GetPublishedVersionFromMemory(sourceInfo);

            if (version < 0) {
                version = this.GetPublishedVersion(sourceInfo);
                this.AddOrUpdatePublishedVersion(sourceInfo, version);
            }

            return version;
        }
    }
}
