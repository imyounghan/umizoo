// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


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
