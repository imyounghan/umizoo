// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

using System;

namespace Umizoo.Infrastructure.Composition
{
    /// <summary>
    /// 标记此特性的类型将要注册到容器中。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RegisterAttribute : Attribute
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public RegisterAttribute()
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public RegisterAttribute(string name)
        {
            Assertions.NotNullOrWhiteSpace(name, "name");
            this.ContactName = name;
        }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public RegisterAttribute(Type type)
        {
            Assertions.NotNull(type, "type");
            this.ContactType = type;
        }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public RegisterAttribute(string name, Type type)
        {
            Assertions.NotNullOrWhiteSpace(name, "name");
            Assertions.NotNull(type, "type");

            this.ContactName = name;
            this.ContactType = type;
        }

        /// <summary>
        /// 注册的名称
        /// </summary>
        public string ContactName { get; private set; }

        /// <summary>
        /// 注册的类型
        /// </summary>
        public Type ContactType { get; private set; }
    }
}