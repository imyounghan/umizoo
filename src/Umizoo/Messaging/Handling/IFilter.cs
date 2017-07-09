

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 表示继承该接口的是一个过滤器
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// 在类中实现时，获取是否允许多个过滤器的值。
        /// </summary>
        bool AllowMultiple { get; }

        /// <summary>
        /// 在类中实现时，获取过滤器顺序。
        /// </summary>
        int Order { get; }
    }
}
