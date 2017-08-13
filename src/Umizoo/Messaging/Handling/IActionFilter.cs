// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 定义操作过滤器中使用的方法。
    /// </summary>
    public interface IActionFilter
    {
        /// <summary>
        /// 在执行操作方法之前调用。
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        void OnActionExecuting(ActionExecutingContext filterContext);

        /// <summary>
        /// 在执行操作方法后调用。
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        void OnActionExecuted(ActionExecutedContext filterContext);
    }
}
