// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using System.Collections;
using System.Runtime.Serialization;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示一个用于发布订阅的异常
    /// </summary>
    public interface IPublishableException : ISerializable, IMessage
    {
        /// <summary>
        ///     错误编码
        /// </summary>
        string ErrorCode { get; }

        /// <summary>
        ///     错误信息
        /// </summary>
        string Message { get; }

        /// <summary>
        ///     获取一个提供用户定义的其他异常信息的键/值对的集合。
        /// </summary>
        IDictionary Data { get; }
    }
}