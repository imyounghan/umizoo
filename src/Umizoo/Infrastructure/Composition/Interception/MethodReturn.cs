using System;
using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// <see cref="IMethodReturn"/> 的实现类
    /// </summary>
    public class MethodReturn : IMethodReturn
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public MethodReturn(IMethodInvocation originalInvocation, object returnValue, object[] arguments)
        {
            originalInvocation.NotNull("originalInvocation");

            this.InvocationContext = originalInvocation.InvocationContext;
            this.ReturnValue = returnValue;
            this.Outputs = new ParameterCollection(arguments, originalInvocation.MethodBase.GetParameters(),
                delegate (ParameterInfo pi) { return pi.ParameterType.IsByRef; });
        }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public MethodReturn(IMethodInvocation originalInvocation, Exception exception)
        {
            originalInvocation.NotNull("originalInvocation");
            exception.NotNull("exception");

            this.InvocationContext = originalInvocation.InvocationContext;
            this.Exception = exception;
            this.Outputs = new ParameterCollection(new object[0], new ParameterInfo[0], delegate { return false; });
        }

        /// <summary>
        /// 获取异常信息
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// 获取当前上下文的数据
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; private set; }
        /// <summary>
        /// 获取返回参数列表
        /// </summary>
        public IParameterCollection Outputs { get; private set; }
        /// <summary>
        /// 获取方法的返回结果
        /// </summary>
        public object ReturnValue { get; private set; }
    }
}
