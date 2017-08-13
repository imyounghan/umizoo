// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.


using System;
using System.Collections.Generic;

namespace Umizoo.Infrastructure.Composition
{
    /// <summary>
    /// 对象容器接口
    /// </summary>
    public interface IObjectContainer
    {
        ICollection<TypeRegistration> RegisteredTypes { get; }

        /// <summary>
        /// 判断此类型是否已注册
        /// </summary>
        bool IsRegistered(Type type, string name = null);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterInstance(Type type, object instance, string name = null);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterType(Type type, string name = null, Lifecycle lifetime = Lifecycle.Singleton);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterType(Type @from, Type to, string name = null, Lifecycle lifetime = Lifecycle.Singleton);

        /// <summary>
        /// 获取类型对应的实例
        /// </summary>
        object Resolve(Type type, string name = null);
        /// <summary>
        /// 获取类型所有的实例
        /// </summary>
        IEnumerable<object> ResolveAll(Type type);

        bool Complete();

        IObjectContainer Parent { get; }

        IObjectContainer CreateChildContainer();
    }
}