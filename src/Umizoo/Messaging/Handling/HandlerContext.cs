// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections;

using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     表示处理程序上下文的类
    /// </summary>
    public class HandlerContext
    {
        /// <summary>
        ///     初始化 <see cref="HandlerContext" /> 类的新实例。
        /// </summary>
        /// <param name="handlerContext">消息处理程序上下文</param>
        protected HandlerContext(HandlerContext handlerContext)
        {
            Assertions.NotNull(handlerContext, "handlerContext");

            Message = handlerContext.Message;
            Handler = handlerContext.Handler;
            InvocationContext = handlerContext.InvocationContext;
        }

        /// <summary>
        ///     初始化 <see cref="HandlerContext" /> 类的新实例。
        /// </summary>
        /// <param name="message">一个消息</param>
        /// <param name="handler">消息处理程序</param>
        public HandlerContext(IMessage message, IHandler handler)
        {
            Assertions.NotNull(message, "message");
            Assertions.NotNull(handler, "handler");

            Message = message;
            Handler = handler;
            InvocationContext = new Hashtable();
        }


        /// <summary>
        ///     当前上下文数据
        /// </summary>
        public IDictionary InvocationContext { get; set; }

        /// <summary>
        ///     要处理的消息
        /// </summary>
        public IMessage Message { get; set; }

        /// <summary>
        ///     处理程序
        /// </summary>
        public IHandler Handler { get; set; }
    }
}