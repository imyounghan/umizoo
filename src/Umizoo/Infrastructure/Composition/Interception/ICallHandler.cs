// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-09.

namespace Umizoo.Infrastructure.Composition.Interception
{
    public interface ICallHandler
    {
        IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext);
    }

    /// <summary>
    /// 获取下一个处理器的委托
    /// </summary>
    public delegate InvokeHandlerDelegate GetNextHandlerDelegate();

    /// <summary>
    /// 表示调用处理器的委托
    /// </summary>
    public delegate IMethodReturn InvokeHandlerDelegate(IMethodInvocation input, GetNextHandlerDelegate getNext);
}
