using System.Threading.Tasks;

namespace Umizoo.Messaging
{
    /// <summary>
    /// 表示这是一个查询服务
    /// </summary>
    public interface IQueryService
    {
        /// <summary>
        /// 读取数据
        /// </summary>
        T Fetch<T>(IQuery query, int timeoutMs = 120000);

        Task<T> FetchAsync<T>(IQuery query, int timeoutMs = 120000);
    }
}
