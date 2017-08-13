// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections;

namespace Umizoo.Infrastructure.Caching
{
    public sealed class HashtableCache : ICache
    {
        private readonly IDictionary hashtable = Hashtable.Synchronized(new Hashtable());

        public HashtableCache(string regionName)
        {
            Region = regionName;
        }

        public object Get(string key)
        {
            return hashtable[key];
        }

        public void Put(string key, object value)
        {
            hashtable[key] = value;
        }

        public void Remove(string key)
        {
            hashtable.Remove(key);
        }

        public void Clear()
        {
            hashtable.Clear();
        }

        public void Destroy()
        {
            hashtable.Clear();
        }

        public string Region { get; }
    }
}