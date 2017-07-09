
namespace Umizoo.Infrastructure.Composition
{
    /// <summary>
    /// 生命周期类型
    /// </summary>
    public enum Lifecycle
    {
        /// <summary>
        /// 单例
        /// </summary>
        Singleton = 0,

        /// <summary>
        /// 每次都构造一个新实例
        /// </summary>
        Transient = 1,
    }
}
