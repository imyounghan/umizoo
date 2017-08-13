// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class MethodReturn : IMethodReturn
    {
        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public MethodReturn(IMethodInvocation originalInvocation, object returnValue, object[] arguments)
        {
            Assertions.NotNull(originalInvocation, "originalInvocation");

            InvocationContext = originalInvocation.InvocationContext;
            ReturnValue = returnValue;
            Outputs = new ParameterCollection(arguments, originalInvocation.MethodBase.GetParameters(),
                delegate(ParameterInfo pi) { return pi.ParameterType.IsByRef; });
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public MethodReturn(IMethodInvocation originalInvocation, Exception exception)
        {
            Assertions.NotNull(originalInvocation, "originalInvocation");
            Assertions.NotNull(exception, "exception");

            InvocationContext = originalInvocation.InvocationContext;
            Exception = exception;
            Outputs = new ParameterCollection(new object[0], new ParameterInfo[0], delegate { return false; });
        }

        /// <summary>
        ///     获取异常信息
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     获取当前上下文的数据
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; }

        /// <summary>
        ///     获取返回参数列表
        /// </summary>
        public IParameterCollection Outputs { get; }

        /// <summary>
        ///     获取方法的返回结果
        /// </summary>
        public object ReturnValue { get; }
    }
}