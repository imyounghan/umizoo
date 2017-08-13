// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;

namespace Umizoo.Infrastructure.Caching
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheRegionAttribute : Attribute
    {
        /// <summary>
        /// 默认的缓存分区名称
        /// </summary>
        public const string DefaultRegion = "Umizoo";


        /// <summary>
        /// 区域名称
        /// </summary>
        public string RegionName { get; set; }
    }
}