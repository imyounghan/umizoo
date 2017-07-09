using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;


namespace Umizoo.Infrastructure.Composition
{

    public class UnityObjectContainer : ObjectContainer
    {
        private readonly IUnityContainer _container;
        public UnityObjectContainer()
            : this(new UnityContainer())
        { }

        public UnityObjectContainer(IUnityContainer container)
        {
            this._container = container;
            container.RegisterInstance<IObjectContainer>(this);
        }

        public override bool IsRegistered(Type type, string name)
        {
            if(string.IsNullOrEmpty(name)) {
                return _container.IsRegistered(type);
            }
            else {
                return _container.IsRegistered(type, name);
            }
        }

        public override void RegisterInstance(Type type, string name, object instance)
        {
            var lifetime = new ContainerControlledLifetimeManager();
            if(string.IsNullOrEmpty(name)) {
                _container.RegisterInstance(type, instance, lifetime);
            }
            else {
                _container.RegisterInstance(type, name, instance, lifetime);
            }
        }

        public override void RegisterType(Type type, string name, Lifecycle lifecycle)
        {
            var lifetime = GetLifetimeManager(lifecycle);

            //var injectionMembers = InterceptionBehaviorMap.Instance.GetBehaviorTypes(type)
            //    .Select(behaviorType => new InterceptionBehavior(behaviorType))
            //    .Cast<InterceptionMember>().ToList();

            //if(injectionMembers.Count > 0) {
            //    if(type.IsSubclassOf(typeof(MarshalByRefObject))) {
            //        injectionMembers.Insert(0, new Interceptor<TransparentProxyInterceptor>());
            //    }
            //    else {
            //        injectionMembers.Insert(0, new Interceptor<VirtualMethodInterceptor>());
            //    }
            //}

            //if(type.IsDefined(typeof(HandlerAttribute), false) ||
            //    type.GetMembers().Any(item => item.IsDefined(typeof(HandlerAttribute), false))) {
            //    int position = injectionMembers.Count > 0 ? 1 : 0;
            //    injectionMembers.Insert(position, new InterceptionBehavior<PolicyInjectionBehavior>());
            //}

            if(string.IsNullOrWhiteSpace(name)) {
                _container.RegisterType(type, lifetime);
            }
            else {
                _container.RegisterType(type, name, lifetime);
            }
        }

        public override void RegisterType(Type from, Type to, string name, Lifecycle lifecycle)
        {
            var lifetimeManager = GetLifetimeManager(lifecycle);

            //var serviceBehaviorTypes = InterceptionBehaviorMap.Instance.GetBehaviorTypes(from);
            //var implBehaviorTypes = InterceptionBehaviorMap.Instance.GetBehaviorTypes(to);

            //var injectionMembers = serviceBehaviorTypes.Union(implBehaviorTypes)
            //    .Select(behaviorType => new InterceptionBehavior(behaviorType))
            //    .Cast<InterceptionMember>().ToList();
            //if(injectionMembers.Count > 0) {
            //    if(implBehaviorTypes.Length > 0) {
            //        if(to.IsSubclassOf(typeof(MarshalByRefObject))) {
            //            injectionMembers.Insert(0, new Interceptor<TransparentProxyInterceptor>());
            //        }
            //        else {
            //            injectionMembers.Insert(0, new Interceptor<VirtualMethodInterceptor>());
            //        }
            //    }
            //    if(serviceBehaviorTypes.Length > 0 && from.IsInterface) {
            //        injectionMembers.Insert(0, new Interceptor<InterfaceInterceptor>());
            //    }
            //}

            //if(to.IsDefined(typeof(HandlerAttribute), false) ||
            //    to.GetMembers().Any(item => item.IsDefined(typeof(HandlerAttribute), false))) {
            //    int position = injectionMembers.Count > 0 ? 1 : 0;
            //    injectionMembers.Insert(position, new InterceptionBehavior<PolicyInjectionBehavior>());
            //}

            if(string.IsNullOrWhiteSpace(name)) {
                _container.RegisterType(from, to, lifetimeManager);
            }
            else {
                _container.RegisterType(from, to, name, lifetimeManager);
            }
        }

        public override object Resolve(Type type, string name)
        {
            if(string.IsNullOrEmpty(name)) {
                return _container.Resolve(type);
            }
            else {
                return _container.Resolve(type, name);
            }
        }

        public override IEnumerable<object> ResolveAll(Type type)
        {
            return _container.ResolveAll(type);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
                _container.Dispose();
        }

        private static LifetimeManager GetLifetimeManager(Lifecycle lifecycle)
        {
            switch(lifecycle) {
                case Lifecycle.Singleton:
                    return new ContainerControlledLifetimeManager();
                default:
                    return new TransientLifetimeManager();
            }
        }
    }
}
