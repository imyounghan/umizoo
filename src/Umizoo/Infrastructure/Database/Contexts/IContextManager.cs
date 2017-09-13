// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;

namespace Umizoo.Infrastructure.Database.Contexts
{
    /// <summary>
    /// 实现上下文的管理接口
    /// </summary>
    public interface IContextManager
    {
        /// <summary>
        /// 上下文工厂标识
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// 获取当前的上下文
        /// </summary>
        ICurrentContext CurrentContext { get; }
    }
}
