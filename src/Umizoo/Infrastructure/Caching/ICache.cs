// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

namespace Umizoo.Infrastructure.Caching
{
    /// <summary>
    /// 缓存接口
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Get the object from the Cache
        /// </summary>
        object Get(string key);

        /// <summary>
        /// Put the object to the Cache
        /// </summary>
        void Put(string key, object value);

        /// <summary>
        /// Remove an item from the Cache.
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Clear the Cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the name of the cache region
        /// </summary>
        string Region { get; }
    }
}