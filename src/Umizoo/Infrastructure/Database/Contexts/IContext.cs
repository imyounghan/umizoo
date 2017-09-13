// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.


namespace Umizoo.Infrastructure.Database.Contexts
{
    /// <summary>
    /// 表示继承该接口的是一个上下文
    /// </summary>
    public interface IContext : System.IDisposable
    {
        /// <summary>
        /// 获取当前的Manager
        /// </summary>
        IContextManager ContextManager { get; }
    }
}
