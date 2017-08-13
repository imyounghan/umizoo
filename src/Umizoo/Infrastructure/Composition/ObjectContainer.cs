// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Umizoo.Infrastructure.Composition
{
    public abstract class ObjectContainer : DisposableObject, IObjectContainer
    {
        private readonly List<TypeRegistration> registeredTypes;

        private int completed;

        protected ObjectContainer(IObjectContainer parentContainer)
        {
            registeredTypes = new List<TypeRegistration>();
            Parent = parentContainer;
        }


        public ICollection<TypeRegistration> RegisteredTypes =>
            new ReadOnlyCollection<TypeRegistration>(registeredTypes);

        public IObjectContainer Parent { get; }

        public virtual bool Complete()
        {
            return Interlocked.Exchange(ref completed, 1) == 1;
        }

        public abstract IObjectContainer CreateChildContainer();

        public abstract bool IsRegistered(Type type, string name);

        public abstract void RegisterInstance(Type type, object instance, string name);

        public virtual void RegisterType(Type type, string name, Lifecycle lifetime)
        {
            RegisterType(type, type, name, lifetime);
        }

        public abstract void RegisterType(Type from, Type to, string name, Lifecycle lifetime);

        public abstract object Resolve(Type type, string name);

        public abstract IEnumerable<object> ResolveAll(Type type);

        bool IObjectContainer.IsRegistered(Type type, string name)
        {
            Assertions.NotNull(type, "type");

            return registeredTypes.Contains(new TypeRegistration(type, name)) || IsRegistered(type, name);
        }


        void IObjectContainer.RegisterInstance(Type type, object instance, string name)
        {
            if (completed >= 1)
                throw new ApplicationException(
                    "can not register type because this container is completed, please invoke before 'Complete' method.");

            Assertions.NotNull(type, "type");
            Assertions.NotNull(instance, "instance");

            var typeRegistration = new TypeRegistration(type, name, instance is IInitializer);

            if (registeredTypes.Contains(typeRegistration) || IsRegistered(type, name))
                throw new ApplicationException(
                    string.Format("the type of '{0}' as name '{1}' has been registered.", type.FullName, name));

            registeredTypes.Add(typeRegistration);
            RegisterInstance(type, instance, name);
        }

        void IObjectContainer.RegisterType(Type type, string name, Lifecycle lifetime)
        {
            if (completed >= 1)
                throw new ApplicationException(
                    "can not register type because this container is completed, please invoke before 'Complete' method.");

            Assertions.NotNull(type, "type");

            if (!type.IsClass || type.IsAbstract)
                throw new ApplicationException(
                    string.Format("the type of '{0}' must be a class and cannot be abstract.", type.FullName));

            var typeRegistration = new TypeRegistration(type, name, lifetime == Lifecycle.Singleton && typeof(IInitializer).IsAssignableFrom(type));

            if (registeredTypes.Contains(typeRegistration) || IsRegistered(type, name))
                throw new ApplicationException(
                    string.Format("the type of '{0}' as name '{1}' has been registered.", type.FullName, name));

            registeredTypes.Add(typeRegistration);
            RegisterType(type, name, lifetime);
        }

        void IObjectContainer.RegisterType(Type from, Type to, string name, Lifecycle lifetime)
        {
            if (completed >= 1)
                throw new ApplicationException(
                    "can not register type because this container is completed, please invoke before 'Complete' method.");

            Assertions.NotNull(from, "from");
            Assertions.NotNull(to, "to");

            if (!to.IsClass || to.IsAbstract)
                throw new ApplicationException(
                    string.Format("the type of '{0}' must be a class and cannot be abstract.", to.FullName));

            if (!from.IsAssignableFrom(to))
                throw new ApplicationException(
                    string.Format("'{0}' does not extend '{1}'.", to.FullName, from.FullName));

            var typeRegistration = new TypeRegistration(from, name, lifetime == Lifecycle.Singleton && typeof(IInitializer).IsAssignableFrom(to));

            if (registeredTypes.Contains(typeRegistration) || IsRegistered(from, name))
                throw new ApplicationException(
                    string.Format("the type of '{0}' as name '{1}' has been registered.", to.FullName, name));

            registeredTypes.Add(typeRegistration);
            RegisterType(from, to, name, lifetime);
        }
    }
}