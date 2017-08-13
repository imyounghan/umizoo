// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using Umizoo.Infrastructure.Filtering;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示拦截器的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class InterceptorAttribute : FilterAttribute
    {
        /// <summary>
        /// 创建拦截器
        /// </summary>
        public abstract IInterceptor CreateInterceptor(IObjectContainer container);
    }
}