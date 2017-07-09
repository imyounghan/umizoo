
namespace Umizoo.Infrastructure.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// <see cref="IObjectContainer" />抽象实现类
    /// </summary>
    public abstract class ObjectContainer : DisposableObject, IObjectContainer
    {
        #region Fields

        private readonly List<TypeRegistration> _registeredTypes;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContainer"/> class. 
        /// </summary>
        protected ObjectContainer()
        {
            this._registeredTypes = new List<TypeRegistration>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// <see cref="IObjectContainer"/> 的一个实例
        /// </summary>
        public static IObjectContainer Instance { get; internal set; }

        /// <summary>
        /// 获取已注册的类型列表
        /// </summary>
        public ReadOnlyCollection<TypeRegistration> RegisteredTypes
        {
            get
            {
                return new ReadOnlyCollection<TypeRegistration>(this._registeredTypes);
            }
        }

        #endregion

        #region Methods and Operators

        /// <summary>
        /// 判断此类型是否已注册
        /// </summary>
        public abstract bool IsRegistered(Type type, string name);

        /// <summary>
        /// 判断此类型是否已注册
        /// </summary>
        public virtual bool IsRegistered(TypeRegistration key)
        {
            return this.IsRegistered(key.Type, key.Name);
        }

        /// <summary>
        /// 注册一个实例
        /// </summary>
        public abstract void RegisterInstance(Type type, string name, object instance);

        /// <summary>
        /// 注册一个实例
        /// </summary>
        public virtual void RegisterInstance(TypeRegistration key, object instance)
        {
            this.RegisterInstance(key.Type, key.Name, instance);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        public virtual void RegisterType(Type type, string name, Lifecycle lifetime)
        {
            this.RegisterType(type, type, name, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        public virtual void RegisterType(TypeRegistration key, Lifecycle lifetime)
        {
            this.RegisterType(key.Type, key.Name, lifetime);
        }

        /// <summary>
        /// 注册一个类型
        /// </summary>
        public abstract void RegisterType(Type from, Type to, string name, Lifecycle lifetime);

        /// <summary>
        /// 注册一个类型
        /// </summary>
        public virtual void RegisterType(TypeRegistration key, Type implType, Lifecycle lifetime)
        {
            this.RegisterType(key.Type, implType, key.Name, lifetime);
        }

        /// <summary>
        /// 获取类型对应的实例
        /// </summary>
        public abstract object Resolve(Type type, string name);

        /// <summary>
        /// 获取类型对应的实例
        /// </summary>
        public virtual object Resolve(TypeRegistration key)
        {
            return this.Resolve(key.Type, key.Name);
        }

        /// <summary>
        /// 获取类型所有的实例
        /// </summary>
        public abstract IEnumerable<object> ResolveAll(Type type);

        #endregion

        #region Explicit Interface Methods

        bool IObjectContainer.IsRegistered(Type type, string name)
        {
            Ensure.NotNull(type, "type");

            return this._registeredTypes.Contains(new TypeRegistration(type, name)) || this.IsRegistered(type, name);
        }

        void IObjectContainer.RegisterInstance(Type type, object instance, string name)
        {
            Ensure.NotNull(type, "type");
            Ensure.NotNull(instance, "instance");

            var typeRegistration = new TypeRegistration(type, name);

            if (this._registeredTypes.Contains(typeRegistration) || this.IsRegistered(type, name))
            {
                throw new ApplicationException(
                    string.Format("the type of '{0}' as name '{1}' has been registered.", type.FullName, name));
            }

            this._registeredTypes.Add(typeRegistration);
            this.RegisterInstance(type, name, instance);
        }

        void IObjectContainer.RegisterType(Type type, string name, Lifecycle lifetime)
        {
            Ensure.NotNull(type, "type");

            if (!type.IsClass || type.IsAbstract)
            {
                throw new ApplicationException(
                    string.Format("the type of '{0}' must be a class and cannot be abstract.", type.FullName));
            }

            var typeRegistration = new TypeRegistration(type, name);

            if (this._registeredTypes.Contains(typeRegistration) || this.IsRegistered(type, name))
            {
                throw new ApplicationException(
                    string.Format("the type of '{0}' as name '{1}' has been registered.", type.FullName, name));
            }

            this._registeredTypes.Add(typeRegistration);
            this.RegisterType(type, name, lifetime);
        }

        void IObjectContainer.RegisterType(Type from, Type to, string name, Lifecycle lifetime)
        {
            Ensure.NotNull(from, "from");
            Ensure.NotNull(to, "to");

            if (!to.IsClass || to.IsAbstract)
            {
                throw new ApplicationException(
                    string.Format("the type of '{0}' must be a class and cannot be abstract.", to.FullName));
            }

            if (!from.IsAssignableFrom(to))
            {
                throw new ApplicationException(
                    string.Format("'{0}' does not extend '{1}'.", to.FullName, from.FullName));
            }

            var typeRegistration = new TypeRegistration(from, name);

            if (this._registeredTypes.Contains(typeRegistration) || this.IsRegistered(from, name))
            {
                throw new ApplicationException(
                    string.Format("the type of '{0}' as name '{1}' has been registered.", to.FullName, name));
            }

            this._registeredTypes.Add(typeRegistration);
            this.RegisterType(from, to, name, lifetime);
        }

        #endregion
    }
}