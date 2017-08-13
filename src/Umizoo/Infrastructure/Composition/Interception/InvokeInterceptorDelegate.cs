// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示调用拦截器的委托
    /// </summary>
    public delegate IMethodReturn InvokeInterceptorDelegate(IMethodInvocation input, GetNextInterceptorDelegate getNext);
}