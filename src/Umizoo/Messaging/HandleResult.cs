// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Runtime.Serialization;
using Umizoo.Configurations;

namespace Umizoo.Messaging
{
    [DataContract]
    public abstract class HandleResult
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public HandleResult()
            : this(HandleStatus.Success)
        {
        }

        /// <summary>
        ///     Parameterized Constructor.
        /// </summary>
        public HandleResult(HandleStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;

            ReplyServer = string.Concat(ConfigurationSettings.InnerAddress,
                "@", ConfigurationSettings.ServiceName);
        }

        [DataMember]
        public string ReplyServer { get; set; }

        /// <summary>
        ///     失败的消息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     返回处理状态。
        /// </summary>
        [DataMember]
        public HandleStatus Status { get; set; }
    }
}