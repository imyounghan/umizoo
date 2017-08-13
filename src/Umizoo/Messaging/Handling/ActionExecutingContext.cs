// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 执行操作方法前调用的上下文
    /// </summary>
    public class ActionExecutingContext : HandlerContext
    {
        /// <summary>
        /// 初始化 <see cref="ActionExecutingContext"/> 类的新实例。
        /// </summary>
        /// <param name="handlerContext">处理程序</param>
        public ActionExecutingContext(HandlerContext handlerContext)
            : base(handlerContext)
        {
            WillExecute = true;
        }

        /// <summary>
        /// 是否继续执行
        /// </summary>
        public bool WillExecute { get; set; }

        /// <summary>
        /// 获取或设置由操作方法返回的结果。
        /// </summary>
        public object ReturnValue { get; set; }
    }
}
