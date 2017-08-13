// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.


using System;
using System.Collections.Generic;
using System.Linq;

namespace Umizoo.Infrastructure.Composition
{
    public static class ObjectContainerExtentions
    {
        /// <summary>
        /// 注册类型
        /// </summary>
        public static void RegisterMultiple(this IObjectContainer that, IEnumerable<Type> registrationTypes, object instance)
        {
            Assertions.NotNull(instance, "instance");

            foreach (var registrationType in registrationTypes) {
                that.RegisterInstance(registrationType, instance);
            }
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static void RegisterMultiple(this IObjectContainer that, Type registrationType, IEnumerable<Type> implementationTypes, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            foreach (var implementationType in implementationTypes) {
                that.RegisterType(registrationType, implementationType, implementationType.FullName, lifecycle);
            }
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static void RegisterMultiple(this IObjectContainer that, IEnumerable<Type> registrationTypes, Type implementationType, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            foreach (var registrationType in registrationTypes) {
                that.RegisterType(registrationType, implementationType, lifecycle);
            }
        }

        /// <summary>
        /// 注册一个实例
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="instance">该类型的实例</param>
        /// <param name="name">注册的名称</param>
        public static void RegisterInstance<T>(this IObjectContainer that, T instance, string name = null)
        {
            Assertions.NotNull(instance, "instance");

            that.RegisterInstance(typeof(T), instance, name);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <param name="that">容器</param>
        /// <param name="type">注册类型</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType(this IObjectContainer that, Type type, Lifecycle lifetime)
        {
            that.RegisterType(type, (string)null, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <param name="that">容器</param>
        /// <param name="from">注册类型</param>
        /// <param name="to">目标类型</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType(this IObjectContainer that, Type from, Type to, Lifecycle lifetime)
        {
            that.RegisterType(from, to, (string)null, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType<T>(this IObjectContainer that, Lifecycle lifetime)
        {
            that.RegisterType<T>((string)null, lifetime);
        }
        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="name">注册的名称</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType<T>(this IObjectContainer that, string name = null, Lifecycle lifetime = Lifecycle.Singleton)
        {
            that.RegisterType(typeof(T), name, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <typeparam name="TFrom">注册类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType<TFrom, TTo>(this IObjectContainer that, Lifecycle lifetime)
            where TTo : TFrom
        {
            that.RegisterType<TFrom, TTo>((string)null, lifetime);
        }
        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <typeparam name="TFrom">注册类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="name">注册的名称</param>
        /// <param name="lifetime">生命周期类型</param>
        public static void RegisterType<TFrom, TTo>(this IObjectContainer that, string name = null, Lifecycle lifetime = Lifecycle.Singleton)
            where TTo : TFrom
        {
            //Ensure.NotWhiteSpace(name, "name");
            that.RegisterType(typeof(TFrom), typeof(TTo), name, lifetime);
        }


        /// <summary>
        /// 判断此类型是否已注册
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="name">注册的名称</param>
        public static bool IsRegistered<T>(this IObjectContainer that, string name = null)
        {
            return that.IsRegistered(typeof(T), name);
        }

        /// <summary>
        /// 获取类型对应的实例
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="name">注册的名称</param>
        public static T Resolve<T>(this IObjectContainer that, string name = null)
        {
            return (T)that.Resolve(typeof(T), name);
        }
        /// <summary>
        /// 获取类型所有的实例
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        public static IEnumerable<T> ResolveAll<T>(this IObjectContainer that)
        {
            return that.ResolveAll(typeof(T)).Cast<T>().ToArray();
        }
    }
}