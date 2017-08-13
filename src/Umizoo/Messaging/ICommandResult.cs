// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

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
