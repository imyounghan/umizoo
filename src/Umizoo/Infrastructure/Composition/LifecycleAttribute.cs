// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

using System;

namespace Umizoo.Infrastructure.Composition
{
    /// <summary>
    /// 表示实例的生命周期的特性(默认为Singleton)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LifecycleAttribute : Attribute
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LifecycleAttribute()
            : this(Lifecycle.Singleton)
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public LifecycleAttribute(Lifecycle lifecycle)
        {
            Lifecycle = lifecycle;
        }

        /// <summary>
        /// 返回生命周期类型(默认为Singleton)
        /// </summary>
        public Lifecycle Lifecycle { get; }
    }
}