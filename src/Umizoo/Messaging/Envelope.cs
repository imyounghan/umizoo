// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


using System;
using System.Collections.Generic;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public abstract class Envelope : EventArgs
    {
    }

    /// <summary>
    ///     提供封装一个对象的信封
    /// </summary>
    public class Envelope<T> : Envelope
    {
        public Envelope()
        {
        }

        /// <summary>
        ///     初始化一个 <see cref="Envelope{T}" /> 类的新实例
        /// </summary>
        public Envelope(T body)
            : this(body, null)
        {
        }

        public Envelope(T body, string messageId)
        {
            Body = body;
            MessageId = messageId;
            Items = new Dictionary<string, object>();
        }

        /// <summary>
        ///     获取该信封的主体对象
        /// </summary>
        public T Body { get; }

        /// <summary>
        ///     获取消息的Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        ///     键值对的集合
        /// </summary>
        public IDictionary<string, object> Items { get; set; }

        public override string ToString()
        {
            return string.Format("{0}({1})#{2}", Body.GetType().FullName, TextSerializer.Instance.Serialize(Body), MessageId);
        }
    }
}