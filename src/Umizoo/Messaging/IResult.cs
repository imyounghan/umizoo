
namespace Umizoo.Messaging
{
    /// <summary>
    /// 处理结果
    /// </summary>
    public interface IResult : IMessage
    {
        /// <summary>
        /// 状态
        /// </summary>
        HandleStatus Status { get; }

        string ErrorMessage { get; }

    }
}
