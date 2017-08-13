// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Umizoo.Infrastructure.Caching
{
    public abstract class CacheProvider : ICacheProvider
    {
        readonly ConcurrentDictionary<string, ICache> _caches = new ConcurrentDictionary<string, ICache>();

        protected virtual ICache BuildCache(string region)
        {
            return BuildCache(region, new Dictionary<string, string>());
        }

        protected abstract ICache BuildCache(string region, IDictionary<string, string> properties);

        public ICache GetCache(string region, IDictionary<string, string> properties)
        {
            return _caches.GetOrAdd(region, key => BuildCache(key, properties));
        }

        public ICache GetCache(string region)
        {
            return _caches.GetOrAdd(region, BuildCache);
        }
    }
}