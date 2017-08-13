// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class MethodInvocation : IMethodInvocation
    {
        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public MethodInvocation(object target, MethodBase methodBase, params object[] parameterValues)
        {
            Assertions.NotNull(target, "target");
            Assertions.NotNull(methodBase, "methodBase");

            Target = target;
            MethodBase = methodBase;
            InvocationContext = new Dictionary<string, object>();

            var targetParameters = methodBase.GetParameters();
            Arguments = new ParameterCollection(parameterValues, targetParameters, param => true);
            Inputs = new ParameterCollection(parameterValues, targetParameters, param => !param.IsOut);
        }

        /// <summary>
        ///     方法上所有的参数列表
        /// </summary>
        public IParameterCollection Arguments { get; }

        /// <summary>
        ///     方法上输入参数的列表
        /// </summary>
        public IParameterCollection Inputs { get; }

        /// <summary>
        ///     当前上下文数据
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; }

        /// <summary>
        ///     调用方法的信息
        /// </summary>
        public MethodBase MethodBase { get; }

        /// <summary>
        ///     调用方法所在的实例
        /// </summary>
        public object Target { get; }

        /// <summary>
        ///     创建一个带有异常信息的结果
        /// </summary>
        /// ram>
        public IMethodReturn CreateExceptionMethodReturn(Exception ex)
        {
            return new MethodReturn(this, ex);
        }

        /// <summary>
        ///     创建一个正确返回的结果
        /// </summary>
        public IMethodReturn CreateMethodReturn(object returnValue, params object[] outputs)
        {
            return new MethodReturn(this, returnValue, outputs);
        }
    }
}