namespace Umizoo.Messaging
{
    public interface IQueryResult : IResult
    {
        /// <summary>
        /// 结果数据
        /// </summary>
        object Data { get; }
    }
}
