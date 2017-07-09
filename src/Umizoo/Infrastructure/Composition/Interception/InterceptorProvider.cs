using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// <see cref="IInterceptorProvider"/> 的实现类
    /// </summary>
    public class InterceptorProvider : IInterceptorProvider
    {
        private readonly IObjectContainer container;

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public InterceptorProvider(IObjectContainer container)
        {
            this.container = container;
        }


        private static FilterComparer _filterComparer = new FilterComparer();

        #region IInterceptorProvider 成员

        /// <summary>
        /// 获取该方法上的拦截器
        /// </summary>
        public IEnumerable<IInterceptor> GetInterceptors(MethodInfo method)
        {
            IEnumerable<Filter> combinedFilters =
                GetFilters(method).OrderBy(filter => filter, _filterComparer);

            return RemoveDuplicates(combinedFilters)
                .Select(filter => filter.Attribute.CreateInterceptor(container));
        }

        #endregion

        private IEnumerable<Filter> GetFilters(MethodInfo method)
        {
            var typeFilters = method.DeclaringType.GetCustomAttributes<InterceptorAttribute>(false)
                .Select(attr => new Filter(attr, FilterScope.Type));
            var methodFilters = method.GetCustomAttributes<InterceptorAttribute>(false)
                .Select(attr => new Filter(attr, FilterScope.Method));

            return typeFilters.Concat(methodFilters);
        }

        private IEnumerable<Filter> RemoveDuplicates(IEnumerable<Filter> filters)
        {
            HashSet<Type> visitedTypes = new HashSet<Type>();

            foreach (Filter filter in filters) {
                Type filterInstanceType = filter.Attribute.GetType();

                if (!visitedTypes.Contains(filterInstanceType) || filter.Attribute.AllowMultiple) {
                    yield return filter;
                    visitedTypes.Add(filterInstanceType);
                }
            }
        }

        private enum FilterScope
        {
            First = 0,
            Global = 10,
            Type = 20,
            Method = 30,
            Last = 100,
        }

        private class Filter
        {
            public Filter(InterceptorAttribute attribute, FilterScope scope)
            {
                attribute.NotNull("attribute");

                this.Attribute = attribute;
                this.Scope = scope;
            }
            public InterceptorAttribute Attribute
            {
                get;
                protected set;
            }

            public int Order
            {
                get { return this.Attribute.Order; }
            }

            public FilterScope Scope
            {
                get;
                protected set;
            }
        }

        private class FilterComparer : IComparer<Filter>
        {
            public int Compare(Filter x, Filter y)
            {
                // Nulls always have to be less than non-nulls
                if (x == null && y == null) {
                    return 0;
                }
                if (x == null) {
                    return -1;
                }
                if (y == null) {
                    return 1;
                }

                // Sort first by order...

                if (x.Order < y.Order) {
                    return -1;
                }
                if (x.Order > y.Order) {
                    return 1;
                }

                // ...then by scope

                if (x.Scope < y.Scope) {
                    return -1;
                }
                if (x.Scope > y.Scope) {
                    return 1;
                }

                return 0;
            }
        }
    }
}
