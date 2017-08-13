// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 执行操作方法后调用的上下文
    /// </summary>
    public class ActionExecutedContext : HandlerContext
    {
        /// <summary>
        /// 初始化 <see cref="ActionExecutedContext"/> 类的新实例。
        /// </summary>
        /// <param name="handlerContext">处理程序上下文</param>
        /// <param name="canceled">如果操作已取消，则为 true</param>
        /// <param name="exception">异常对象</param>
        public ActionExecutedContext(HandlerContext handlerContext, bool canceled, Exception exception)
            : base(handlerContext)
        {
            Canceled = canceled;
            Exception = exception;
        }

        /// <summary>
        /// 获取或设置一个值，该值指示此 <see cref="ActionExecutedContext"/> 对象已被取消。
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// 获取或设置在操作方法的执行过程中发生的异常（如果有）。
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示是否处理异常。
        /// </summary>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// 获取或设置由操作方法返回的结果。
        /// </summary>
        public object ReturnValue { get; set; }
    }
}
