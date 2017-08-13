// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;

namespace Umizoo.Infrastructure.Async
{
    public delegate void EndInvokeDelegate(IAsyncResult asyncResult);

    public delegate TResult EndInvokeDelegate<TResult>(IAsyncResult asyncResult);
}