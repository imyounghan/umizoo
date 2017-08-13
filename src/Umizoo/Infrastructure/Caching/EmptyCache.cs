// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


namespace Umizoo.Infrastructure.Caching
{
    public sealed class EmptyCache : ICache
    {
        public static readonly EmptyCache Instance = new EmptyCache();

        private EmptyCache()
        { }

        public string Region { get { return null; } }

        public void Clear()
        {
        }

        public object Get(string key)
        {
            return null;
        }

        public void Put(string key, object value)
        {
        }

        public void Remove(string key)
        {
        }
    }
}