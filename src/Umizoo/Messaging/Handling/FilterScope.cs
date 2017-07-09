

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 定义值，这些值指定过滤器在同一过滤器类型和过滤器顺序内的运行顺序。
    /// </summary>
    public enum FilterScope
    {
        /// <summary>
        /// 指定第一个
        /// </summary>
        First = 0,

        /// <summary>
        /// 指定在 <see cref="FilterScope.Handler"/> 之前、<see cref="FilterScope.First"/> 之后的顺序。
        /// </summary>
        Global = 10,

        /// <summary>
        /// 指定在 <see cref="FilterScope.Action"/> 之前、<see cref="FilterScope.Global"/> 之后的顺序。
        /// </summary>
        Handler = 20,

        /// <summary>
        /// 指定在 <see cref="FilterScope.Last"/> 之前、<see cref="FilterScope.Handler"/> 之后的顺序。
        /// </summary>
        Action = 30,

        /// <summary>
        /// 指定最后一个。
        /// </summary>
        Last = 100,
    }
}
