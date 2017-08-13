// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

namespace Umizoo.Infrastructure.Composition
{
    /// <summary>
    /// 对象生命周期类型
    /// </summary>
    public enum Lifecycle
    {
        /// <summary>
        /// 单例
        /// </summary>
        Singleton = 0,

        /// <summary>
        /// 每次都构造一个新实例
        /// </summary>
        Transient = 1,
    }
}