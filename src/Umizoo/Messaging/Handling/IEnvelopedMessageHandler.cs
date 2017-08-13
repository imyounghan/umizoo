// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 表示继承该接口的是一个信封处理程序
    /// </summary>
    public interface IEnvelopedHandler : IHandler { }


    /// <summary>
    /// 表示继承该接口的是一个信封消息的处理程序
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public interface IEnvelopedMessageHandler<TMessage> : IEnvelopedHandler
        where TMessage : class, IMessage
    {
        /// <summary>
        /// 处理信封消息
        /// </summary>
        /// <param name="envelope">封装一个消息对象的信封</param>
        void Handle(Envelope<TMessage> envelope);
    }
}
