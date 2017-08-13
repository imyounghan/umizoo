// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Collections.Generic;

namespace Umizoo.Infrastructure.Caching
{
    public sealed class EmptyCacheProvider : ICacheProvider
    {
        public ICache GetCache(string region, IDictionary<string, string> properties)
        {
            return EmptyCache.Instance;
        }

        public ICache GetCache(string region)
        {
            return EmptyCache.Instance;
        }
    }
}