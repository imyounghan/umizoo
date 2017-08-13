// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表现一个拦截器的接口
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// 调用结果
        /// </summary>
        IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptorDelegate getNext);
    }
}