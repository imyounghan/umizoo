

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Umizoo.Infrastructure;

    /// <summary>
    /// 表示应用程序的过滤器提供程序的集合。
    /// </summary>
    public class FilterProviderCollection : Collection<IFilterProvider>
    {
        private static FilterComparer _filterComparer = new FilterComparer();

        /// <summary>
        /// 返回过滤器提供程序的集合。
        /// </summary>
        /// <param name="handerContext">处理程序上下文</param>
        /// <returns>过滤器提供程序的集合</returns>
        public IEnumerable<Filter> GetFilters(HandlerContext handlerContext)
        {
            Ensure.NotNull(handlerContext, "handlerContext");

            IEnumerable<Filter> combinedFilters =
                Items.SelectMany(fp => fp.GetFilters(handlerContext))
                             .OrderBy(filter => filter, _filterComparer);

            // Remove duplicates from the back forward
            return RemoveDuplicates(combinedFilters.Reverse()).Reverse();
        }

        private static IEnumerable<Filter> RemoveDuplicates(IEnumerable<Filter> filters)
        {
            HashSet<Type> visitedTypes = new HashSet<Type>();

            foreach(Filter filter in filters) {
                object filterInstance = filter.Instance;
                Type filterInstanceType = filterInstance.GetType();

                if(!visitedTypes.Contains(filterInstanceType) || AllowMultiple(filterInstance)) {
                    yield return filter;
                    visitedTypes.Add(filterInstanceType);
                }
            }
        }

        private static bool AllowMultiple(object filterInstance)
        {
            IFilter filter = filterInstance as IFilter;
            if(filter == null) {
                return true;
            }

            return filter.AllowMultiple;
        }

        private class FilterComparer : IComparer<Filter>
        {
            public int Compare(Filter x, Filter y)
            {
                // Nulls always have to be less than non-nulls
                if(x == null && y == null) {
                    return 0;
                }
                if(x == null) {
                    return -1;
                }
                if(y == null) {
                    return 1;
                }

                // Sort first by order...

                if(x.Order < y.Order) {
                    return -1;
                }
                if(x.Order > y.Order) {
                    return 1;
                }

                // ...then by scope

                if(x.Scope < y.Scope) {
                    return -1;
                }
                if(x.Scope > y.Scope) {
                    return 1;
                }

                return 0;
            }
        }
    }
}
