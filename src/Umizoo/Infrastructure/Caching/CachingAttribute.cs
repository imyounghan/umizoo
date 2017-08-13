// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;

namespace Umizoo.Infrastructure.Caching
{
    /// <summary>
    /// 表示由此特性所描述的方法，能够获得框架所提供的缓存功能。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class CachingAttribute : CacheRegionAttribute
    {
        /// <summary>
        /// 获取或设置缓存方式。
        /// </summary>
        public CachingMethod Method { get; set; }

        /// <summary>
        /// 缓存标识
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// 获取与当前缓存方式相关的区域名称。注：此参数仅在缓存方式为Remove时起作用。
        /// </summary>
        public string[] RelatedAreas { get; set; }
    }
}