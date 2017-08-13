// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections.Generic;

namespace Umizoo.Infrastructure.Caching
{
    /// <summary>
    /// 缓存支持
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// 配置缓存
        /// </summary>
        /// <param name="region">缓存区域的名称</param>
        /// <param name="properties">配置项</param>
        ICache GetCache(string region, IDictionary<string, string> properties);

        /// <summary>
        /// 配置缓存
        /// </summary>
        /// <param name="region">缓存区域的名称</param>
        ICache GetCache(string region);
    }
}