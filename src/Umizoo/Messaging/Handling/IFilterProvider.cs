

namespace Umizoo.Messaging.Handling
{
    using System.Collections.Generic;

    /// <summary>
    /// 提供用于查找过滤器的接口。
    /// </summary>
    public interface IFilterProvider
    {
        /// <summary>
        /// 返回一个包含服务定位器中的所有 <see cref="IFilterProvider"/> 实例的枚举器。
        /// </summary>
        /// <param name="handlerContext">处理程序上下文</param>
        /// <returns>包含服务定位器中的所有 <see cref="IFilterProvider"/> 实例的枚举器。</returns>
        IEnumerable<Filter> GetFilters(HandlerContext handlerContext);
    }
}
