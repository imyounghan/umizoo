// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections.Generic;

namespace Umizoo.Infrastructure.Caching
{
    /// <summary>
    /// .Net Hashtable
    /// </summary>
    public class HashtableCacheProvider : CacheProvider
    {
        protected override ICache BuildCache(string region, IDictionary<string, string> properties)
        {
            return new HashtableCache(region);
        }
    }
}