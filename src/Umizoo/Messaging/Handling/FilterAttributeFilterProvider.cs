

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// 定义过滤器特性的过滤器提供程序。
    /// </summary>
    public class FilterAttributeFilterProvider : IFilterProvider
    {
        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>> _methodFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>();
        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>> _typeFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>();

        static ICollection<FilterAttribute> GetTypeFilterAttributes(Type handlerType)
        {
            return _typeFilterAttributeCache.GetOrAdd(handlerType, delegate(Type type) {
                var attributes = type.GetAllAttributes<FilterAttribute>(false).ToList();
                return new ReadOnlyCollection<FilterAttribute>(attributes);
            });
        }

        static ICollection<FilterAttribute> GetMethodFilterAttributes(Type handlerType, Type messageType)
        {
            return _methodFilterAttributeCache.GetOrAdd(messageType, delegate {
                var mi = handlerType.GetMethod("Handle", new Type[] { messageType });
                if (mi == null) {
                    mi = handlerType.GetMethod("Handle", new Type[] { typeof(ICommandContext), messageType });
                }
                var attributes = mi.GetAllAttributes<FilterAttribute>(false).ToList();
                return new ReadOnlyCollection<FilterAttribute>(attributes);
            });
        }

        /// <summary>
        /// 将所有过滤器提供程序中的过滤器聚合为一个集合。
        /// </summary>
        /// <param name="commandHandlerContext">处理程序上下文</param>
        /// <returns>来自所有过滤器提供程序的过滤器的集合。</returns>
        public IEnumerable<Filter> GetFilters(HandlerContext commandHandlerContext)
        {
            if(commandHandlerContext == null) {
                return Enumerable.Empty<Filter>();
            }

            var handlerType = commandHandlerContext.Handler.GetType();
            var messageType = commandHandlerContext.Message.GetType();

            var typeFilters = GetTypeFilterAttributes(handlerType)
                .Select(attr => new Filter(attr, FilterScope.Handler, attr.Order));
            var methodFilters = GetMethodFilterAttributes(handlerType, messageType)
                .Select(attr => new Filter(attr, FilterScope.Action, attr.Order));

            return typeFilters.Concat(methodFilters);
        }
    }
}
