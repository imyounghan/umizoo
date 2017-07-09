using System;
using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// <see cref="IMethodInvocation"/> 的实现类
    /// </summary>
    public class MethodInvocation : IMethodInvocation
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public MethodInvocation(object target, MethodBase methodBase, params object[] parameterValues)
        {
            Ensure.NotNull(target, "target");
            Ensure.NotNull(methodBase, "methodBase");

            this.Target = target;
            this.MethodBase = methodBase;
            this.InvocationContext = new Dictionary<string, object>();

            ParameterInfo[] targetParameters = methodBase.GetParameters();
            this.Arguments = new ParameterCollection(parameterValues, targetParameters, param => true);
            this.Inputs = new ParameterCollection(parameterValues, targetParameters, param => !param.IsOut);
        }

        /// <summary>
        /// 方法上所有的参数列表
        /// </summary>
        public IParameterCollection Arguments { get; private set; }
        /// <summary>
        /// 方法上输入参数的列表
        /// </summary>
        public IParameterCollection Inputs { get; private set; }
        /// <summary>
        /// 当前上下文数据
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; private set; }
        /// <summary>
        /// 调用方法的信息
        /// </summary>
        public MethodBase MethodBase { get; private set; }
        /// <summary>
        /// 调用方法所在的实例
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// 创建一个带有异常信息的结果
        /// </summary>ram>
        public IMethodReturn CreateExceptionMethodReturn(Exception ex)
        {
            return new MethodReturn(this, ex);
        }
        /// <summary>
        /// 创建一个正确返回的结果
        /// </summary>
        public IMethodReturn CreateMethodReturn(object returnValue, params object[] outputs)
        {
            return new MethodReturn(this, returnValue, outputs);
        }
    }
}
