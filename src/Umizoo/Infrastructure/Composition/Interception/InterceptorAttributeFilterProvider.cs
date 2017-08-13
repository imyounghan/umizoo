// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Umizoo.Infrastructure.Filtering;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class InterceptorAttributeFilterProvider : IFilterProvider<MethodBase>
    {
        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<InterceptorAttribute>> _methodFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<InterceptorAttribute>>();
        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<InterceptorAttribute>> _typeFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<InterceptorAttribute>>();

        static ICollection<InterceptorAttribute> GetTypeInterceptorAttributes(Type handlerType)
        {
            return _typeFilterAttributeCache.GetOrAdd(handlerType, delegate (Type type) {
                var attributes = type.GetAllAttributes<InterceptorAttribute>(false).ToList();
                return new ReadOnlyCollection<InterceptorAttribute>(attributes);
            });
        }

        static ICollection<InterceptorAttribute> GetMethodInterceptorAttributes(MethodBase method)
        {
            return _methodFilterAttributeCache.GetOrAdd(method.ReflectedType, delegate {
                var attributes = method.GetAllAttributes<InterceptorAttribute>(false).ToList();
                return new ReadOnlyCollection<InterceptorAttribute>(attributes);
            });
        }

        public IEnumerable<Filter> GetFilters(MethodBase method)
        {
            if (method == null) {
                return Enumerable.Empty<Filter>();
            }

            var typeFilters = GetTypeInterceptorAttributes(method.ReflectedType)
                .Select(attr => new Filter(attr, FilterScope.Class, attr.Order));
            var methodFilters = GetMethodInterceptorAttributes(method)
                .Select(attr => new Filter(attr, FilterScope.Method, attr.Order));

            return typeFilters.Concat(methodFilters);
        }
    }
}