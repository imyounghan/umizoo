
namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示继承该接口的是具有提供关键字的功能
    /// </summary>
    public interface IRoutingProvider
    {
        /// <summary>
        /// 获取关键字
        /// </summary>
        string GetRoutingKey();
    }
}
