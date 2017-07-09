

namespace Umizoo.Messaging
{
    public interface ICommandResult : IResult
    {
        /// <summary>
        /// 错误编码
        /// </summary>
        string ErrorCode { get; }

        /// <summary>
        /// 错误消息
        /// </summary>
        string Result { get; }
        
    }
}
