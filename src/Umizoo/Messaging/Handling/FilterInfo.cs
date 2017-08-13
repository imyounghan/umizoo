// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections.Generic;
using System.Linq;
using Umizoo.Infrastructure.Filtering;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     封装有关可用的操作过滤器的信息。
    /// </summary>
    public class FilterInfo
    {
        private readonly List<IActionFilter> actionFilters;
        private readonly List<IExceptionFilter> exceptionFilters;

        /// <summary>
        ///     初始化 <see cref="FilterInfo" /> 类的新实例。
        /// </summary>
        /// <param name="filters">过滤器集合</param>
        public FilterInfo(IEnumerable<Filter> filters)
        {
            actionFilters = new List<IActionFilter>();
            exceptionFilters = new List<IExceptionFilter>();

            var filterInstances = filters.Select(f => f.Instance).ToList();

            actionFilters.AddRange(filterInstances.OfType<IActionFilter>());
            exceptionFilters.AddRange(filterInstances.OfType<IExceptionFilter>());
        }

        /// <summary>
        ///     获取应用程序中的所有操作过滤器。
        /// </summary>
        public IList<IActionFilter> ActionFilters => actionFilters;

        /// <summary>
        ///     获取应用程序中的所有异常过滤器。
        /// </summary>
        public IList<IExceptionFilter> ExceptionFilters => exceptionFilters;
    }
}