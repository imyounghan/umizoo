

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 将控制器添加到 <see cref="FilterProviderCollection"/> 实例。
    /// </summary>
    public class HandlerInstanceFilterProvider : IFilterProvider
    {
        /// <summary>
        /// 返回过滤器实例筛选器的集合。
        /// </summary>
        /// <param name="handlerContext"></param>
        /// <returns>处理程序过滤器的集合。</returns>
        public IEnumerable<Filter> GetFilters(HandlerContext handlerContext)
        {
            if(handlerContext.Handler != null) {
                yield return new Filter(handlerContext.Handler,
                    FilterScope.Global,
                    Int32.MinValue);
            }
        }
    }
}
