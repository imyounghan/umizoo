// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Umizoo.Infrastructure.Filtering;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     定义过滤器特性的过滤器提供程序。
    /// </summary>
    public class FilterAttributeFilterProvider : IFilterProvider<HandlerContext>
    {
        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>
            _methodFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>();

        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>
            _typeFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>();

        /// <summary>
        ///     将所有过滤器提供程序中的过滤器聚合为一个集合。
        /// </summary>
        /// <param name="handlerContext">处理程序上下文</param>
        /// <returns>来自所有过滤器提供程序的过滤器的集合。</returns>
        public IEnumerable<Filter> GetFilters(HandlerContext handlerContext)
        {
            if (handlerContext == null) return Enumerable.Empty<Filter>();

            var handlerType = handlerContext.Handler.GetType();
            var messageType = handlerContext.Message.GetType();

            var typeFilters = GetTypeFilterAttributes(handlerType)
                .Select(attr => new Filter(attr, FilterScope.Class, attr.Order));
            var methodFilters = GetMethodFilterAttributes(handlerType, messageType)
                .Select(attr => new Filter(attr, FilterScope.Method, attr.Order));

            return typeFilters.Concat(methodFilters);
        }

        private static ICollection<FilterAttribute> GetTypeFilterAttributes(Type handlerType)
        {
            return _typeFilterAttributeCache.GetOrAdd(handlerType, delegate(Type type)
            {
                var attributes = type.GetAllAttributes<FilterAttribute>(false).ToList();
                return new ReadOnlyCollection<FilterAttribute>(attributes);
            });
        }

        private static ICollection<FilterAttribute> GetMethodFilterAttributes(Type handlerType, Type messageType)
        {
            return _methodFilterAttributeCache.GetOrAdd(messageType, delegate
            {
                var mi = handlerType.GetMethod("Handle", new[] {messageType});
                if (mi == null) mi = handlerType.GetMethod("Handle", new[] {typeof(ICommandContext), messageType});
                var attributes = mi.GetAllAttributes<FilterAttribute>(false).ToList();
                return new ReadOnlyCollection<FilterAttribute>(attributes);
            });
        }
    }
}