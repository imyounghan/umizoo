// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


using System.Runtime.Serialization;

namespace Umizoo.Messaging
{
    [DataContract]
    public class CommandResult : HandleResult, ICommandResult
    {
        /// <summary>
        ///     操作超时
        /// </summary>
        public static readonly CommandResult Timeout = new CommandResult(HandleStatus.Timeout, "Operation is timeout.");

        /// <summary>
        ///     发送失败
        /// </summary>
        public static readonly CommandResult SentFailed = new CommandResult(HandleStatus.Failed, "Send to bus failed.");

        /// <summary>
        ///     发送成功
        /// </summary>
        public static readonly CommandResult SentSuccess = new CommandResult {ReplyType = CommandReturnMode.Delivered};

        /// <summary>
        ///     命令执行完成
        /// </summary>
        public static readonly CommandResult CommandExecuted =
            new CommandResult {ReplyType = CommandReturnMode.CommandExecuted};

        /// <summary>
        ///     命令生成的事件处理完成
        /// </summary>
        public static readonly CommandResult EventHandled =
            new CommandResult {ReplyType = CommandReturnMode.EventHandled};

        /// <summary>
        ///     手动
        /// </summary>
        public static readonly CommandResult Finished = new CommandResult {ReplyType = CommandReturnMode.Manual};


        /// <summary>
        ///     Default constructor.
        /// </summary>
        public CommandResult()
            : this(HandleStatus.Success)
        {
        }

        /// <summary>
        ///     Parameterized Constructor.
        /// </summary>
        public CommandResult(HandleStatus status, string errorMessage = null, string errorCode = "-1")
            : base(status, errorMessage)
        {
            ErrorCode = errorCode;
        }

        [DataMember]
        public CommandReturnMode ReplyType { get; set; }

        [DataMember]
        public int ProducedEventCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ResultType { get; set; }

        /// <summary>
        ///     错误编码
        /// </summary>
        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Result { get; set; }
    }
}