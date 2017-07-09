
namespace Umizoo.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Umizoo.Communication;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Composition;
    using Umizoo.Messaging;
    using Umizoo.Messaging.Handling;
    using Umizoo.Seeds;

    /// <summary>
    ///     引导程序
    /// </summary>
    public class Configuration
    {
        #region Static Fields

        /// <summary>
        ///     当前配置
        /// </summary>
        public static readonly Configuration Current = new Configuration();

        #endregion

        #region Fields

        private readonly Stopwatch _stopwatch;
        private List<Assembly> _assemblies;
        private HashSet<Component> _components;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class. 
        /// </summary>
        protected Configuration()
        {
            this._assemblies = new List<Assembly>();
            this._components = new HashSet<Component>();
            this._stopwatch = Stopwatch.StartNew();
            this.Status = ServerStatus.Running;
        }

        #endregion

        #region Enums

        /// <summary>
        ///     服务状态
        /// </summary>
        public enum ServerStatus
        {
            /// <summary>
            ///     运行中
            /// </summary>
            Running,

            /// <summary>
            ///     已启动
            /// </summary>
            Started,

            /// <summary>
            ///     已停止
            /// </summary>
            Stopped
        }


        #endregion

        #region Public Properties

        /// <summary>
        ///     当前服务器状态
        /// </summary>
        public ServerStatus Status { get; private set; }

        public IDictionary<string, Type> CommandTypes { get; private set; }

        public IDictionary<string, Type> EventTypes { get; private set; }

        public IDictionary<string, Type> AggregateTypes { get; private set; }

        public IDictionary<string, Type> PublishableExceptionTypes { get; private set; }

        public IDictionary<string, Type> QueryTypes { get; private set; }

        public IDictionary<string, Type> ResultTypes { get; private set; }
        #endregion

        #region Methods and Operators

        /// <summary>
        ///     配置完成。
        /// </summary>
        public void Done()
        {
            this.Done(new DefaultObjectContainer());
        }

        /// <summary>
        /// 配置完成。
        /// </summary>
        public void Done(IObjectContainer container)
        {
            if (this.Status != ServerStatus.Running)
            {
                return;
            }

            if (this._assemblies.Count == 0)
            {
                this.LoadAssemblies();

                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat("load assemblies completed. [{0}]",
                       string.Join("; ", _assemblies.Select(assembly => assembly.FullName)));
                }
            }

            ObjectContainer.Instance = container;

            Type[] nonAbstractTypes =
                this._assemblies.SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .ToArray();

            this.CommandTypes =
                nonAbstractTypes.Where(type => typeof(ICommand).IsAssignableFrom(type))
                    .ToDictionary(type => type.Name, type => type);
            this.EventTypes =
                nonAbstractTypes.Where(type => typeof(IEvent).IsAssignableFrom(type))
                    .ToDictionary(type => type.Name, type => type);
            this.PublishableExceptionTypes =
                nonAbstractTypes.Where(type => typeof(IPublishableException).IsAssignableFrom(type))
                    .ToDictionary(type => type.Name, type => type);
            this.AggregateTypes =
                nonAbstractTypes.Where(type => typeof(IEventSourced).IsAssignableFrom(type))
                    .ToDictionary(type => type.Name, type => type);
            this.QueryTypes =
                nonAbstractTypes.Where(type => typeof(IQuery).IsAssignableFrom(type))
                    .ToDictionary(type => type.Name, type => type);
            this.ResultTypes =
                nonAbstractTypes.Where(type => typeof(IResult).IsAssignableFrom(type))
                    .ToDictionary(type => type.Name, type => type);


            this.RegisterComponents(nonAbstractTypes);
            this.RegisterHandlerAndFetcher(nonAbstractTypes);

            // this.OnAssembliesLoaded(_assemblies, nonAbstractTypes);
            this.RegisterDefaultComponents();

            List<Component> initializerComponents = new List<Component>();
            foreach (var component in _components)
            {
                component.Register(container);
                if (component.MustbeInitialize())
                {
                    initializerComponents.Add(component);
                }
            }

            HashSet<IInitializer> initializers = new HashSet<IInitializer>();
            foreach (var component in initializerComponents)
            {
                var initializer = component.GetInstance(container) as IInitializer;
                if (initializer != null)
                {
                    initializers.Add(initializer);
                }
            }

            foreach (var initializer in initializers)
            {
                initializer.Initialize(container, this._assemblies);
            }


            this._assemblies.Clear();
            this._components.Clear();

            this._assemblies = null;
            this._components = null;

            this.Start();

            this._stopwatch.Stop();

            LogManager.Default.InfoFormat("system is working, used time:{0}ms.", this._stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        public Configuration LoadAssemblies(Assembly[] assemblies)
        {
            this._assemblies.Clear();
            this._assemblies.AddRange(assemblies);

            return this;
        }

        /// <summary>
        /// 设置组件
        /// </summary>
        public Configuration SetDefault(Type type, object instance, string name = null)
        {
            if (this.Status != ServerStatus.Running) {
                throw new ApplicationException(
                    "system is working, can not register type, please execute before 'Done' method.");
            }

            Ensure.NotNull(type, "type");

            this._components.Add(new Component(type, name, instance));

            return this;
        }

        /// <summary>
        /// 设置组件
        /// </summary>
        public Configuration SetDefault(Type type, string name, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            if (this.Status != ServerStatus.Running) {
                throw new ApplicationException(
                    "system is working, can not register type, please execute before 'Done' method.");
            }

            Ensure.NotNull(type, "type");

            this._components.Add(new Component(type, name, lifecycle));

            return this;
        }

        /// <summary>
        /// 设置组件
        /// </summary>
        public Configuration SetDefault(Type from, Type to, string name, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            if (this.Status != ServerStatus.Running) {
                throw new ApplicationException(
                    "system is working, can not register type, please execute before 'done' method.");
            }

            Ensure.NotNull(from, "type");
            Ensure.NotNull(to, "type");

            this._components.Add(new Component(from, to, name, lifecycle));

            return this;
        }

        /// <summary>
        ///     启动相关Processes
        /// </summary>
        public virtual void Start()
        {
            if (this.Status == ServerStatus.Started) {
                return;
            }

            ObjectContainer.Instance.ResolveAll<IProcessor>().ForEach(p => p.Start());

            this.Status = ServerStatus.Started;
        }

        /// <summary>
        ///     停止相关Processes
        /// </summary>
        public virtual void Stop()
        {
            if (this.Status == ServerStatus.Stopped) {
                return;
            }

            ObjectContainer.Instance.ResolveAll<IProcessor>().ForEach(p => p.Stop());

            this.Status = ServerStatus.Stopped;
        }


        private static bool FilterType(Type type)
        {
            if (!type.IsGenericType) {
                return false;
            }

            Type genericType = type.GetGenericTypeDefinition();

            return IsMessageHandlerInterfaceType(genericType) || IsQueryHandlerInterfaceType(genericType);
        }

        private static bool IsQueryHandlerInterfaceType(Type genericType)
        {
            return genericType == typeof(IQueryHandler<,>);
        }

        private static bool IsMessageHandlerInterfaceType(Type genericType)
        {
            return genericType == typeof(IMessageHandler<>) || genericType == typeof(IEnvelopedMessageHandler<>)
                   || genericType == typeof(ICommandHandler<>) || genericType == typeof(IEventHandler<>);
        }

        private void RegisterComponents(IEnumerable<Type> types)
        {
            IEnumerable<Type> registionTypes = types.Where(p => p.IsDefined(typeof(RegisterAttribute), false));

            foreach (Type type in registionTypes) {
                Lifecycle lifecycle = LifecycleAttribute.GetLifecycle(type);

                var attribute = type.GetSingleAttribute<RegisterAttribute>(false);
                if (attribute != null) {
                    Type contractType = attribute.ServiceType;
                    string contractName = attribute.Name;
                    if (contractType == null) {
                        this.SetDefault(type, contractName, lifecycle);
                    }
                    else {
                        this.SetDefault(contractType, type, contractName, lifecycle);
                    }
                }
            }
        }


        private void RegisterDefaultComponents()
        {
            this.SetDefault(DefaultTextSerializer.Instance);
            this.SetDefault<IEventStore, EventStoreInMemory>();
            this.SetDefault<IEventPublishedVersionStore, EventPublishedVersionInMemory>();
            this.SetDefault<ISnapshotStore, NoneSnapshotStore>();
            this.SetDefault<ICache, LocalCache>();
            this.SetDefault<IRepository, MemoryRepository>();
            this.SetDefault<IMessageBus<IResult>, MessageProducer<IResult>>();
            this.SetDefault<IMessageReceiver<Envelope<IResult>>, MessageProducer<IResult>>();
            this.SetDefault<IResultManager, ResultManager>();
        }

        private void RegisterHandlerAndFetcher(IEnumerable<Type> types)
        {
            foreach (Type type in types) {
                Lifecycle lifecycle = LifecycleAttribute.GetLifecycle(type);

                Type[] interfaceTypes = type.GetInterfaces();
                foreach (Type interfaceType in interfaceTypes.Where(FilterType)) {
                    this.SetDefault(interfaceType, type, type.FullName, lifecycle);
                }
            }

            InnerHandlerProvider.Instance.Initialize(types);
        }

        #endregion


        private class Component
        {
            #region Constructors and Destructors

            public Component(Type type, string name, object instance)
            {
                this.ContractKey = new TypeRegistration(type, name);
                this.Instance = instance;
                this.Lifecycle = Lifecycle.Singleton;
            }

            public Component(Type type, string name, Lifecycle lifecycle)
                : this(type, type, name, lifecycle)
            {
            }

            public Component(Type from, Type to, string name, Lifecycle lifecycle)
            {
                this.ContractKey = new TypeRegistration(from, name);
                this.ImplementationType = to;
                this.Lifecycle = lifecycle;
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     要注册的类型
            /// </summary>
            public TypeRegistration ContractKey { get; set; }

            /// <summary>
            ///     要注册类型的实现类型
            /// </summary>
            public Type ImplementationType { get; set; }

            /// <summary>
            ///     要注册类型的实例
            /// </summary>
            public object Instance { get; private set; }

            /// <summary>
            ///     生命周期
            /// </summary>
            public Lifecycle Lifecycle { get; private set; }

            #endregion

            #region Methods and Operators

            /// <summary>
            /// 返回一个值，该值指示此实例是否与指定的对象相等。
            /// </summary>
            public override bool Equals(object obj)
            {
                var other = obj as Component;

                if (other == null) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return this.ContractKey.Equals(other.ContractKey);
            }

            /// <summary>
            /// 返回此实例的哈希代码。
            /// </summary>
            public override int GetHashCode()
            {
                return this.ContractKey.GetHashCode();
            }

            public override string ToString()
            {
                return this.ContractKey.ToString();
            }

            internal object GetInstance(IObjectContainer container)
            {
                if (this.Instance != null) {
                    return this.Instance;
                }

                return container.Resolve(this.ContractKey);
            }

            internal bool MustbeInitialize()
            {
                return this.Lifecycle == Lifecycle.Singleton
                       && (IsInitializeType(this.ImplementationType) || (this.Instance is IInitializer));
            }

            internal void Register(IObjectContainer container)
            {
                if (container.IsRegistered(this.ContractKey)) {
                    return;
                }

                if (this.Instance != null) {
                    container.RegisterInstance(this.ContractKey, this.Instance);
                    return;
                }

                container.RegisterType(this.ContractKey, this.ImplementationType, this.Lifecycle);
            }

            private static bool IsInitializeType(Type type)
            {
                return type != null && type.IsClass && !type.IsAbstract && typeof(IInitializer).IsAssignableFrom(type);
            }

            #endregion
        }
    }

    /// <summary>
    ///     <see cref="Configuration" /> 的扩展类
    /// </summary>
    public static class ConfigurationExtentions
    {
        #region Public Methods and Operators

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault(
            this Configuration that,
            Type type,
            Lifecycle lifecycle = Lifecycle.Singleton)
        {
            return that.SetDefault(type, null, lifecycle);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault(
            this Configuration that,
            Type from,
            Type to,
            Lifecycle lifecycle = Lifecycle.Singleton)
        {
            return that.SetDefault(from, to, null, lifecycle);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault<T>(this Configuration that, T instance, string name = null)
        {
            return that.SetDefault(typeof(T), instance, name);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault<T>(this Configuration that, Lifecycle lifecycle = Lifecycle.Singleton)
        {
            return that.SetDefault<T>((string)null, lifecycle);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault<T>(
            this Configuration that,
            string name,
            Lifecycle lifecycle = Lifecycle.Singleton)
        {
            return that.SetDefault(typeof(T), name, lifecycle);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault<TFrom, TTo>(
            this Configuration that,
            Lifecycle lifecycle = Lifecycle.Singleton) where TTo : TFrom
        {
            return that.SetDefault<TFrom, TTo>((string)null, lifecycle);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public static Configuration SetDefault<TFrom, TTo>(
            this Configuration that,
            string name,
            Lifecycle lifecycle = Lifecycle.Singleton) where TTo : TFrom
        {
            return that.SetDefault(typeof(TFrom), typeof(TTo), name, lifecycle);
        }

        #endregion

        /// <summary>
        /// 加载程序集，如果为空就扫描目录
        /// </summary>
        public static Configuration LoadAssemblies(this Configuration that, params string[] assemblyNames)
        {
            Assembly[] assemblies = new Assembly[0];

            if (assemblyNames == null || assemblyNames.Length > 0) {
                assemblies = assemblyNames.Select(Assembly.Load).ToArray();
            }
            else {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
                string binPath = string.IsNullOrEmpty(relativeSearchPath)
                                     ? baseDir
                                     : Path.Combine(baseDir, relativeSearchPath);

                assemblies = Directory.GetFiles(binPath).Where(
                    file => {
                        string ext = Path.GetExtension(file).ToLower();
                        return ext.EndsWith(".dll") || ext.EndsWith(".exe");
                    }).Select(Assembly.LoadFrom).ToArray();
            }

            return that.LoadAssemblies(assemblies);
        }

        public static Configuration UseLocalQueue(this Configuration that, ProcessingFlags flags = ProcessingFlags.All)
        {
            if (flags == ProcessingFlags.All || (flags & ProcessingFlags.Command) == ProcessingFlags.Command) {
                that.SetDefault<IMessageBus<ICommand>, MessageProducer<ICommand>>();
                that.SetDefault<IMessageReceiver<Envelope<ICommand>>, MessageProducer<ICommand>>();
            }
            if (flags == ProcessingFlags.All || (flags & ProcessingFlags.Event) == ProcessingFlags.Event) {
                that.SetDefault<IMessageBus<IEvent>, MessageProducer<IEvent>>();
                that.SetDefault<IMessageReceiver<Envelope<IEvent>>, MessageProducer<IEvent>>();
            }
            if (flags == ProcessingFlags.All || (flags & ProcessingFlags.PublishableException) == ProcessingFlags.PublishableException) {
                that.SetDefault<IMessageBus<IPublishableException>, MessageProducer<IPublishableException>>();
                that.SetDefault<IMessageReceiver<Envelope<IPublishableException>>, MessageProducer<IPublishableException>>();
            }
            if(flags == ProcessingFlags.All || (flags & ProcessingFlags.Query) == ProcessingFlags.Query) {
                that.SetDefault<IMessageBus<IQuery>, MessageProducer<IQuery>>();
                that.SetDefault<IMessageReceiver<Envelope<IQuery>>, MessageProducer<IQuery>>();
            }


            return that.EnableProcessors(flags);
        }

        public static Configuration EnableProcessors(this Configuration that, 
            ProcessingFlags processingFlags = ProcessingFlags.All, 
            ConnectionMode connectionMode = ConnectionMode.Local)
        {
            if(processingFlags == ProcessingFlags.All || (processingFlags & ProcessingFlags.Command) == ProcessingFlags.Command) {
                that.SetDefault<IProcessor, CommandConsumer>("command");
            }
            if(processingFlags == ProcessingFlags.All || (processingFlags & ProcessingFlags.Event) == ProcessingFlags.Event) {
                that.SetDefault<IProcessor, EventConsumer>("event");
            }
            if(processingFlags == ProcessingFlags.All || (processingFlags & ProcessingFlags.PublishableException) == ProcessingFlags.PublishableException) {
                that.SetDefault<IProcessor, PublishableExceptionConsumer>("exception");
            }
            if(processingFlags == ProcessingFlags.All || (processingFlags & ProcessingFlags.Query) == ProcessingFlags.Query) {
                that.SetDefault<IProcessor, QueryConsumer>("query");
            }

            switch(connectionMode) {
                case ConnectionMode.Wcf:
                    that.SetDefault<IProcessor, ResultConsumerWithWcf>("result");
                    break;
                case ConnectionMode.Socket:
                    that.SetDefault<IProcessor, ResultConsumerWithSocket>("result");
                    break;
            }

            return that;
        }

        public static Configuration EnableService(this Configuration that, ConnectionMode connectionMode = ConnectionMode.Local)
        {
            that.SetDefault<IProcessor, ResultConsumerWithLocal>("result");

            switch (connectionMode)
            {
                case ConnectionMode.Local:
                    that.UseLocalQueue();
                    that.SetDefault<ICommandService, CentralService>();
                    that.SetDefault<IQueryService, CentralService>();
                    break;
                case ConnectionMode.Wcf:
                    that.SetDefault<IProcessor, WcfRequestServer>("requestService");
                    that.SetDefault<IProcessor, WcfReplyServer>("replyService");
                    break;
                case ConnectionMode.Socket:
                    that.SetDefault<IProcessor, SocketRequestServer>("requestService");
                    that.SetDefault<IProcessor, SocketReplyServer>("replyService");
                    break;
            }

            return that;
        }        
    }
}