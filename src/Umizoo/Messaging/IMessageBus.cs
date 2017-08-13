// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections.Generic;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示一个消息总线的接口
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public interface IMessageBus<TMessage> where TMessage : IMessage
    {
        void Send(Envelope<TMessage> message);

        void Send(IEnumerable<Envelope<TMessage>> messages);
    }
}