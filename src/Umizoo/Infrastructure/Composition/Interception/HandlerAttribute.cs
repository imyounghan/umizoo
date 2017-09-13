// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-09.

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示拦截器的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class HandlerAttribute : Attribute//, IInterceptor
    {
        private readonly static ConcurrentDictionary<Type, bool> multiuseAttributeCache = new ConcurrentDictionary<Type, bool>();

        private int order = -1;


        private static bool AllowsMultiple(Type attributeType)
        {
            return multiuseAttributeCache.GetOrAdd(
                attributeType,
                type => type.GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                    .Cast<AttributeUsageAttribute>()
                    .First()
                    .AllowMultiple
            );
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否可指定拦截器特性的多个实例。
        /// </summary>
        public bool AllowMultiple
        {
            get
            {
                return AllowsMultiple(this.GetType());
            }
        }

        /// <summary>
        /// 获取或者设置执行操作拦截器的顺序。
        /// </summary>
        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", "Order must be greater than or equal to -1.");
                }
                this.order = value;
            }
        }

        public abstract ICallHandler CreateHandler(IObjectContainer container);
    }
}