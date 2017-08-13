// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.


using System;

namespace Umizoo.Infrastructure.Composition
{
    public class TypeRegistration
    {
        public TypeRegistration(Type type, bool initialization)
            : this(type, null, initialization)
        {
        }

        public TypeRegistration(Type type, string name, bool initialization)
            : this(type, name)
        {
            InitializationRequired = initialization;
        }

        public TypeRegistration(Type type, string name)
        {
            Assertions.NotNull(type, "type");
            Type = type;
            Name = name;
        }

        /// <summary>
        ///     名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     表示需要初始化该实例
        /// </summary>
        public bool InitializationRequired { get; }

        public override bool Equals(object obj)
        {
            if (obj.IsNull())
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as TypeRegistration;
            if (other == null)
                return false;

            if (Type != other.Type)
                return false;

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + (string.IsNullOrWhiteSpace(Name) ? 0 : Name.GetHashCode());
        }
    }
}