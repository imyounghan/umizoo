// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 定义异常过滤器所需的方法。
    /// </summary>
    public interface IExceptionFilter
    {
        /// <summary>
        /// 在发生异常时调用。
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        void OnException(ExceptionContext filterContext);
    }
}
