// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections.Generic;
using Umizoo.Infrastructure.Filtering;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     将控制器添加到 <see cref="FilterProviderCollection" /> 实例。
    /// </summary>
    public class HandlerInstanceFilterProvider : IFilterProvider<HandlerContext>
    {
        /// <summary>
        ///     返回过滤器实例筛选器的集合。
        /// </summary>
        /// <param name="handlerContext"></param>
        /// <returns>处理程序过滤器的集合。</returns>
        public IEnumerable<Filter> GetFilters(HandlerContext handlerContext)
        {
            if (handlerContext.Handler != null)
                yield return new Filter(handlerContext.Handler,
                    FilterScope.Global,
                    int.MinValue);
        }
    }
}