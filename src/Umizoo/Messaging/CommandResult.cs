
namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;

    [DataContract]
    public class CommandResult : HandleResult, ICommandResult
    {
        /// <summary>
        /// 操作超时
        /// </summary>
        public static readonly CommandResult Timeout = new CommandResult(HandleStatus.Timeout, "Operation is timeout.");

        public static readonly CommandResult SentFailed = new CommandResult(HandleStatus.Failed, "Send to bus failed.");

        public static readonly CommandResult Delivered = new CommandResult() { ReplyType = CommandReturnMode.Delivered };
        public static readonly CommandResult CommandExecuted = new CommandResult() { ReplyType = CommandReturnMode.CommandExecuted };
        public static readonly CommandResult EventHandled = new CommandResult() { ReplyType = CommandReturnMode.EventHandled };
        public static readonly CommandResult ManualCompleted = new CommandResult() { ReplyType = CommandReturnMode.Manual };


        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommandResult()
            : this(HandleStatus.Success, null, "1")
        {
        }

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public CommandResult(HandleStatus status, string errorMessage = null, string errorCode = "-1")
            : base(status, errorMessage)
        {
            this.ErrorCode = errorCode;
        }

        [DataMember]
        public CommandReturnMode ReplyType { get; set; }

        [DataMember]
        public int ProduceEventCount { get; set; }

        /// <summary>
        /// 错误编码
        /// </summary>
        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Result { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ResultType { get; set; }
    }
}
