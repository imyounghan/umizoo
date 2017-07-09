
namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示继续该接口的是一个有标识的对象
    /// </summary>
    public interface IUniquelyIdentifiable
    {
        /// <summary>
        /// 标识ID
        /// </summary>
        string Id { get; }
    }
}
