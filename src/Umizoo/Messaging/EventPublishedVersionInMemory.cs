// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Concurrent;

namespace Umizoo.Messaging
{
    public class EventPublishedVersionInMemory : IEventPublishedVersionStore
    {
        private readonly ConcurrentDictionary<SourceInfo, int>[] _versionCaches;

        public EventPublishedVersionInMemory()
            : this(5)
        {
        }

        protected EventPublishedVersionInMemory(int dictCount)
        {
            _versionCaches = new ConcurrentDictionary<SourceInfo, int>[dictCount];
            for (var index = 0; index < dictCount; index++)
                _versionCaches[index] = new ConcurrentDictionary<SourceInfo, int>();
        }

        void IEventPublishedVersionStore.AddOrUpdatePublishedVersion(SourceInfo sourceInfo, int version)
        {
            AddOrUpdatePublishedVersionToMemory(sourceInfo, version);
            AddOrUpdatePublishedVersion(sourceInfo, version);
        }

        int IEventPublishedVersionStore.GetPublishedVersion(SourceInfo sourceInfo)
        {
            var version = GetPublishedVersionFromMemory(sourceInfo);

            if (version < 0)
            {
                version = GetPublishedVersion(sourceInfo);
                AddOrUpdatePublishedVersion(sourceInfo, version);
            }

            return version;
        }

        public virtual void AddOrUpdatePublishedVersion(SourceInfo sourceInfo, int version)
        {
        }

        public virtual int GetPublishedVersion(SourceInfo sourceInfo)
        {
            return 0;
        }

        private int GetPublishedVersionFromMemory(SourceInfo sourceKey)
        {
            var dict = _versionCaches[Math.Abs(sourceKey.GetHashCode() % _versionCaches.Length)];
            int version;
            if (dict.TryGetValue(sourceKey, out version)) return version;

            return -1;
        }

        private void AddOrUpdatePublishedVersionToMemory(SourceInfo sourceKey, int version)
        {
            var dict = _versionCaches[Math.Abs(sourceKey.GetHashCode() % _versionCaches.Length)];

            dict.AddOrUpdate(sourceKey,
                version,
                (key, value) => version == value + 1 ? version : value);
        }
    }
}