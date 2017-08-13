// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Umizoo.Infrastructure.Filtering
{
    public class FilterProviderCollection : Collection<IFilterProvider>
    {
        /// <summary>
        ///     返回过滤器提供程序的集合。
        /// </summary>
        public IEnumerable<Filter> GetFilters<T>(T instance)
        {
            Assertions.NotNull(instance, "instance");

            IEnumerable<Filter> combinedFilters =
                Items.OfType<IFilterProvider<T>>().SelectMany(fp => fp.GetFilters(instance))
                    .OrderBy(filter => filter, FilterComparer.Default);

            // Remove duplicates from the back forward
            return RemoveDuplicates(combinedFilters.Reverse()).Reverse();
        }

        private static IEnumerable<Filter> RemoveDuplicates(IEnumerable<Filter> filters)
        {
            var visitedTypes = new HashSet<Type>();

            foreach (var filter in filters)
            {
                var filterInstance = filter.Instance;
                var filterInstanceType = filterInstance.GetType();

                if (!visitedTypes.Contains(filterInstanceType) || AllowMultiple(filterInstance))
                {
                    yield return filter;
                    visitedTypes.Add(filterInstanceType);
                }
            }
        }

        private static bool AllowMultiple(object filterInstance)
        {
            var filter = filterInstance as IFilter;
            if (filter == null)
                return true;

            return filter.AllowMultiple;
        }
    }
}