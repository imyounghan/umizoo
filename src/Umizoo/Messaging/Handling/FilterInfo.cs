

namespace Umizoo.Messaging.Handling
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 封装有关可用的操作过滤器的信息。
    /// </summary>
    public class FilterInfo
    {
        private List<IActionFilter> actionFilters;
        private List<IExceptionFilter> exceptionFilters;

        /// <summary>
        /// 初始化 <see cref="FilterInfo"/> 类的新实例。
        /// </summary>
        /// <param name="filters">过滤器集合</param>
        public FilterInfo(IEnumerable<Filter> filters)
        {
            this.actionFilters = new List<IActionFilter>();
            this.exceptionFilters = new List<IExceptionFilter>();

            var filterInstances = filters.Select(f => f.Instance).ToList();

            this.actionFilters.AddRange(filterInstances.OfType<IActionFilter>());
            this.exceptionFilters.AddRange(filterInstances.OfType<IExceptionFilter>());
        }

        /// <summary>
        /// 获取应用程序中的所有操作过滤器。
        /// </summary>
        public IList<IActionFilter> ActionFilters
        {
            get
            {
                return this.actionFilters;
            }
        }

        /// <summary>
        /// 获取应用程序中的所有异常过滤器。
        /// </summary>
        public IList<IExceptionFilter> ExceptionFilters
        {
            get
            {
                return this.exceptionFilters;
            }
        }
    }
}
