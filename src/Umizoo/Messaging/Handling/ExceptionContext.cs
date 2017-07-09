

namespace Umizoo.Messaging.Handling
{
    using System;

    /// <summary>
    /// 执行操作方法遇到错误的上下文
    /// </summary>
    public class ExceptionContext : HandlerContext
    {
        /// <summary>
        /// 初始化 <see cref="ExceptionContext"/> 类的新实例。
        /// </summary>
        /// <param name="handlerContext">处理程序</param>
        /// <param name="exception">异常对象</param>
        public ExceptionContext(HandlerContext handlerContext, Exception exception)
            : base(handlerContext)
        {
            Exception = exception;
        }


        /// <summary>
        /// 获取异常信息
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示是否处理异常。
        /// </summary>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// 获取或设置操作结果。
        /// </summary>
        public object ReturnValue { get; set; }
    }
}
