// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition
{
    public sealed class DefaultObjectContainer : ObjectContainer
    {
        private readonly Dictionary<Type, ObjectBuilder> _typeBuilderMap;
        private readonly Dictionary<TypeRegistration, Type> _typeImplementerMap;
        private readonly Dictionary<TypeRegistration, object> _typeInstanceMap;
        private readonly Dictionary<Type, ICollection<TypeRegistration>> _typeRegistrationMap;


        public DefaultObjectContainer()
            : this(null)
        {
        }

        private DefaultObjectContainer(IObjectContainer container)
            : base(container)
        {
            _typeInstanceMap = new Dictionary<TypeRegistration, object>();
            _typeBuilderMap = new Dictionary<Type, ObjectBuilder>();
            _typeImplementerMap = new Dictionary<TypeRegistration, Type>();
            _typeRegistrationMap = new Dictionary<Type, ICollection<TypeRegistration>>();
        }


        public override IObjectContainer CreateChildContainer()
        {
            return new DefaultObjectContainer(this);
        }

        public override bool IsRegistered(Type type, string name)
        {
            return false;
        }

        public override void RegisterInstance(Type type, object instance, string name)
        {
            var typeRegistration = new TypeRegistration(type, name);
            if (_typeInstanceMap.TryAdd(typeRegistration, instance))
                _typeRegistrationMap.GetOrAdd(type, () => new List<TypeRegistration>()).Add(typeRegistration);
        }

        public override void RegisterType(Type from, Type to, string name, Lifecycle lifetime)
        {
            var typeRegistration = new TypeRegistration(from, name);
            if (_typeImplementerMap.TryAdd(typeRegistration, to))
            {
                _typeRegistrationMap.GetOrAdd(from, () => new List<TypeRegistration>()).Add(typeRegistration);
                _typeBuilderMap.TryAdd(to, new ObjectBuilder(to, lifetime, this));
            }
        }

        public override object Resolve(Type type, string name)
        {
            return Resolve(new TypeRegistration(type, name));
        }

        private object Resolve(TypeRegistration typeRegistration)
        {
            object instance = null;
            if (!_typeInstanceMap.TryGetValue(typeRegistration, out instance))
            {
                Type implementerType;
                if (_typeImplementerMap.TryGetValue(typeRegistration, out implementerType))
                {
                    ObjectBuilder objectBuilder;
                    if (_typeBuilderMap.TryGetValue(implementerType, out objectBuilder))
                        instance = objectBuilder.GetInstance();
                }
            }

            return instance;
        }

        public override IEnumerable<object> ResolveAll(Type type)
        {
            if (!_typeRegistrationMap.ContainsKey(type))
                return Enumerable.Empty<object>();

            return _typeRegistrationMap[type].Select(Resolve).Distinct().ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            _typeInstanceMap.Clear();
            _typeBuilderMap.Clear();
            _typeImplementerMap.Clear();
            _typeRegistrationMap.Clear();
        }


        private class ObjectBuilder
        {
            private readonly ConstructorInfo _constructorInfo;
            private readonly Lifecycle _lifecycle;
            private readonly IObjectContainer _container;
            private object _instance;


            public ObjectBuilder(Type type, Lifecycle lifecycle, IObjectContainer container)
            {
                var constructors = type.GetConstructors();
                if (constructors.Length == 0)
                {
                    var errorMessage = string.Format("Type '{0}' must have a public constructor.", type.FullName);
                    throw new SystemException(errorMessage);
                }

                if (constructors.Length > 1)
                {
                    var errorMessage = string.Format(
                        "Type '{0}' must have multiple public constructor.",
                        type.FullName);
                    throw new SystemException(errorMessage);
                }

                _constructorInfo = constructors.First();
                _lifecycle = lifecycle;
                _container = container;
            }

            public object GetInstance()
            {
                if (_lifecycle == Lifecycle.Singleton)
                {
                    if (_instance == null)
                        _instance = CreateInstance();

                    return _instance;
                }

                return _instance;
            }

            private object CreateInstance()
            {
                var parameters = _constructorInfo.GetParameters();
                if (parameters.Length == 0)
                    return _constructorInfo.Invoke(new object[0]);

                var args = parameters.Select(GetParameterValue).ToArray();
                return _constructorInfo.Invoke(args);
            }

            private object GetParameterValue(ParameterInfo parameterInfo)
            {
                if (parameterInfo.RawDefaultValue == DBNull.Value)
                    return _container.Resolve(parameterInfo.ParameterType);

                return parameterInfo.RawDefaultValue;
            }
        }
    }
}