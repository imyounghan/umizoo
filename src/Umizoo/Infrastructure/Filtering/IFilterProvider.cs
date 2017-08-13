// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Collections.Generic;

namespace Umizoo.Infrastructure.Filtering
{
    public interface IFilterProvider { }

    /// <summary>
    /// 提供用于查找过滤器的接口。
    /// </summary>
    /// <typeparam name="T">用于定位的类型</typeparam>
    public interface IFilterProvider<T> : IFilterProvider
    {
        /// <summary>
        /// 返回一个包含服务定位器中的所有 <see cref="IFilterProvider{T}"/> 实例的枚举器。
        /// </summary>
        /// <param name="instance"><see cref="T"/> 的实例</param>
        /// <returns>包含服务定位器中的所有 <see cref="IFilterProvider{T}"/> 实例的枚举器。</returns>
        IEnumerable<Filter> GetFilters(T instance);
    }
}