// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Umizoo.Infrastructure.Caching;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Logging;
using Umizoo.Infrastructure;
using Umizoo.Messaging;
using Umizoo.Messaging.Handling;
using Umizoo.Seeds;

namespace Umizoo.Configurations
{
    /// <summary>
    ///     引导程序
    /// </summary>
    public class Configuration : Processor
    {
        public const ProcessingFlags AllProcessingFlags = ProcessingFlags.Command | ProcessingFlags.Event | ProcessingFlags.PublishableException | ProcessingFlags.Result | ProcessingFlags.Query;

        public static IDictionary<string, Type> CommandTypes { get; private set; }

        public static IDictionary<string, Type> EventTypes { get; private set; }

        public static IDictionary<string, Type> AggregateTypes { get; private set; }

        public static IDictionary<string, Type> PublishableExceptionTypes { get; private set; }

        public static IDictionary<string, Type> QueryTypes { get; private set; }

        //public static IDictionary<string, Type> ResultTypes { get; private set; }

        private readonly IObjectContainer _container;
        private List<Assembly> _assemblies;
        private Stopwatch _stopwatch;

        private Configuration(IObjectContainer container)
        {
            _assemblies = new List<Assembly>();
            _stopwatch = Stopwatch.StartNew();
            _container = container;
        }

        public static Configuration Create()
        {
            return Create(new DefaultObjectContainer());
        }

        public static Configuration Create(IObjectContainer container)
        {
            return new Configuration(container);
        }

        public Configuration Accept(Action<IObjectContainer> action)
        {
            action(_container);
            return this;
        }

        /// <summary>
        ///     配置完成。
        /// </summary>
        public void Done()
        {
            if (_assemblies.IsNull())
                return;

            if (_assemblies.Count == 0)
            {
                this.LoadAssemblies();

                if (LogManager.Default.IsDebugEnabled)
                    LogManager.Default.DebugFormat("load assemblies completed. [{0}]",
                        string.Join("; ", _assemblies.Select(assembly => assembly.FullName)));
            }

            var allTypes = _assemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();
            var nonAbstractTypes = allTypes.Where(type => type.IsClass && !type.IsAbstract).ToArray();

            CommandTypes = nonAbstractTypes.Where(typeof(ICommand).IsAssignableFrom)
                .ToDictionary(type => type.Name, type => type);
            EventTypes = nonAbstractTypes.Where(typeof(IEvent).IsAssignableFrom)
                .ToDictionary(type => type.Name, type => type);
            AggregateTypes = nonAbstractTypes.Where(typeof(IAggregateRoot).IsAssignableFrom)
                .ToDictionary(type => type.Name, type => type);
            PublishableExceptionTypes = nonAbstractTypes.Where(typeof(IPublishableException).IsAssignableFrom)
                .ToDictionary(type => type.Name, type => type);
            QueryTypes = nonAbstractTypes.Where(typeof(IQuery).IsAssignableFrom)
                .ToDictionary(type => type.Name, type => type);

            RegisterComponents(nonAbstractTypes);
            RegisterDefaultComponents();
            RegisterHandler(nonAbstractTypes);

            _container.Complete();

            _container.RegisteredTypes.Where(component => component.InitializationRequired)
                .Select(component => _container.Resolve(component.Type, component.Name))
                .OfType<IInitializer>()
                .Distinct()
                .ForEach(initializer => initializer.Initialize(_container, allTypes));


            _assemblies.Clear();
            _assemblies = null;

            Start();

            _stopwatch.Stop();

            LogManager.Default.InfoFormat("system is working, used time:{0}ms.", _stopwatch.ElapsedMilliseconds);

            _stopwatch = null;
        }

        /// <summary>
        ///     加载程序集
        /// </summary>
        public Configuration LoadAssemblies(Assembly[] assemblies)
        {
            _assemblies.Clear();
            _assemblies.AddRange(assemblies);

            return this;
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override void Start()
        {
            _container.ResolveAll<IProcessor>().ForEach(p => p.Start());
        }

        protected override void Stop()
        {
            _container.ResolveAll<IProcessor>().ForEach(p => p.Stop());
        }

        private static Lifecycle GetLifecycle(Type type)
        {
            var attribute = type.GetSingleAttribute<LifecycleAttribute>(false);
            if (!attribute.IsNull())
                return attribute.Lifecycle;

            return Lifecycle.Singleton;
        }

        private void RegisterComponents(IEnumerable<Type> types)
        {
            types.Where(p => p.IsDefined(typeof(RegisterAttribute), false)).ForEach(implementerType =>
            {
                var lifecycle = GetLifecycle(implementerType);

                var attribute = implementerType.GetSingleAttribute<RegisterAttribute>(false);
                if (attribute.ContactType == null)
                    _container.RegisterType(implementerType, attribute.ContactName, lifecycle);
                else _container.RegisterType(attribute.ContactType, implementerType, attribute.ContactName, lifecycle);
            });
        }


        private void RegisterDefaultComponents()
        {
            _container.RegisterInstance(TextSerializer.Instance);
            _container.RegisterType<IEventStore, EventStoreInMemory>();
            _container.RegisterType<IEventPublishedVersionStore, EventPublishedVersionInMemory>();
            _container.RegisterType<ISnapshotStore, NoneSnapshotStore>();
            _container.RegisterType<ICacheProvider, HashtableCacheProvider>();
            _container.RegisterType<IRepository, Repository>();
            _container.RegisterType<IResultManager, ResultManager>();
        }

        private static bool IsHandlerInterface(Type interfaceType)
        {
            if (!interfaceType.IsGenericType) return false;

            var genericType = interfaceType.GetGenericTypeDefinition();

            return genericType == typeof(ICommandHandler<>) || genericType == typeof(IEventHandler<>) ||
                   genericType == typeof(IMessageHandler<>) || genericType == typeof(IEnvelopedMessageHandler<>) ||
                   genericType == typeof(IQueryHandler<,>);
        }

        private void RegisterHandler(IEnumerable<Type> types)
        {
            types.Where(typeof(IHandler).IsAssignableFrom).ForEach(handlerType =>
            {
                if (handlerType == typeof(ResultNotifyHandler) || handlerType == typeof(ResultReplyHandler))
                    return;

                handlerType.GetInterfaces().Where(IsHandlerInterface).ForEach(contractType =>
                {
                    _container.RegisterType(contractType, handlerType, handlerType.FullName);
                });
            });

            AggregateInnerHandlerProvider.Current.Initialize(types);
        }
    }
}