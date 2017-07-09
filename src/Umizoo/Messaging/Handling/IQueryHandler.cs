
namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 继承该接口的是查询执行器
    /// </summary>
    public interface IQueryHandler<TQuery, TResult> : IHandler
        where TQuery : IQuery
    {
        /// <summary>
        /// 获取结果
        /// </summary>
        TResult Handle(TQuery parameter);
    }    
}
