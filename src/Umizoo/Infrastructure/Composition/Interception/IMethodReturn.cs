// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections.Generic;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示这是一个方法返回的接口
    /// </summary>
    public interface IMethodReturn
    {
        /// <summary>
        /// 获取异常信息
        /// </summary>
        Exception Exception { get; }
        /// <summary>
        /// 获取当前上下文的数据
        /// </summary>
        IDictionary<string, object> InvocationContext { get; }
        /// <summary>
        /// 输出参数集合
        /// </summary>
        IParameterCollection Outputs { get; }
        /// <summary>
        /// 返回值
        /// </summary>
        object ReturnValue { get; }
    }
}