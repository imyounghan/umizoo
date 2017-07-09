

namespace Umizoo.Infrastructure.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 对象容器接口
    /// </summary>
    public interface IObjectContainer
    {
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
        void RegisterType(Type type, string name, Lifecycle lifetime = Lifecycle.Singleton);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        void RegisterType(Type from, Type to, string name, Lifecycle lifetime = Lifecycle.Singleton);

        /// <summary>
        /// 获取类型对应的实例
        /// </summary>
        object Resolve(Type type, string name = null);
        /// <summary>
        /// 获取类型所有的实例
        /// </summary>
        IEnumerable<object> ResolveAll(Type type);
    }

    /// <summary>
    /// <see cref="IObjectContainer"/> 扩展方法
    /// </summary>
    public static class ObjectContainerExtentions
    {
        /// <summary>
        /// 注册类型
        /// </summary>
        public static void RegisterMultiple(this IObjectContainer that, IEnumerable<Type> registrationTypes, object instance)
        {
            foreach(var registrationType in registrationTypes) {
                that.RegisterInstance(registrationType, instance);
            }
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static void RegisterMultiple(this IObjectContainer that, Type registrationType, IEnumerable<Type> implementationTypes, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            foreach(var implementationType in implementationTypes) {
                that.RegisterType(registrationType, implementationType, implementationType.FullName, lifecycle);
            }
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public static void RegisterMultiple(this IObjectContainer that, IEnumerable<Type> registrationTypes, Type implementationType, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            foreach(var registrationType in registrationTypes) {
                that.RegisterType(registrationType, implementationType, lifecycle);
            }
        }

        /// <summary>
        /// 注册一个实例
        /// </summary>
        public static void RegisterInstance(this IObjectContainer that, TypeRegistration key, object instance)
        {
            var container = that as ObjectContainer;
            if (container != null) {
                container.RegisterInstance(key, instance);
                return;
            }

            that.RegisterInstance(key.Type, instance, key.Name);
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
            that.RegisterInstance(typeof(T), instance, name);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        public static void RegisterType(this IObjectContainer that, TypeRegistration key, Lifecycle lifetime = Lifecycle.Singleton)
        {
            var container = that as ObjectContainer;
            if (container != null) {
                container.RegisterType(key, lifetime);
                return;
            }

            that.RegisterType(key.Type, key.Name, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <param name="that">容器</param>
        /// <param name="type">注册类型</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType(this IObjectContainer that, Type type, Lifecycle lifetime = Lifecycle.Singleton)
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
        public static void RegisterType(this IObjectContainer that, TypeRegistration key, Type implType, Lifecycle lifetime = Lifecycle.Singleton)
        {
            var container = that as ObjectContainer;
            if (container != null) {
                container.RegisterType(key, implType, lifetime);
                return;
            }

            that.RegisterType(key.Type, implType, key.Name, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <param name="that">容器</param>
        /// <param name="from">注册类型</param>
        /// <param name="to">目标类型</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType(this IObjectContainer that, Type from, Type to, Lifecycle lifetime = Lifecycle.Singleton)
        {
            that.RegisterType(from, to, (string)null, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <typeparam name="T">注册类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType<T>(this IObjectContainer that, Lifecycle lifetime = Lifecycle.Singleton)
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
        public static void RegisterType<T>(this IObjectContainer that, string name, Lifecycle lifetime = Lifecycle.Singleton)
        {
            //Ensure.NotWhiteSpace(name, "name");
            that.RegisterType(typeof(T), name, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        /// <typeparam name="TFrom">注册类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="that">容器</param>
        /// <param name="lifetime">生命周期</param>
        public static void RegisterType<TFrom, TTo>(this IObjectContainer that, Lifecycle lifetime = Lifecycle.Singleton)
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
        public static void RegisterType<TFrom, TTo>(this IObjectContainer that, string name, Lifecycle lifetime = Lifecycle.Singleton)
             where TTo : TFrom
        {
            //Ensure.NotWhiteSpace(name, "name");
            that.RegisterType(typeof(TFrom), typeof(TTo), name, lifetime);
        }

        /// <summary>
        /// 判断此类型是否已注册
        /// </summary>
        public static bool IsRegistered(this IObjectContainer that, TypeRegistration key)
        {
            var container = that as ObjectContainer;
            if (container != null) {
                return container.IsRegistered(key);
            }
            return that.IsRegistered(key.Type, key.Name);
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
        public static object Resolve(this IObjectContainer that, TypeRegistration key)
        {
            var container = that as ObjectContainer;
            if (container != null) {
                return container.Resolve(key);
            }
            return that.Resolve(key.Type, key.Name);
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
