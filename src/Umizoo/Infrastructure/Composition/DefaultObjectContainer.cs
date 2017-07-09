

namespace Umizoo.Infrastructure.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The default object container.
    /// </summary>
    public sealed class DefaultObjectContainer : ObjectContainer
    {
        #region Fields

        private readonly Dictionary<TypeRegistration, object> instances;
        private readonly Dictionary<TypeRegistration, Type> keyToImpltypeMap;
        private readonly Dictionary<Type, ObjectBuilder> objectBuilders;
        private readonly Dictionary<Type, ICollection<TypeRegistration>> typeToAllMap;

        #endregion

        #region Constructors and Destructors

        public DefaultObjectContainer()
        {
            this.instances = new Dictionary<TypeRegistration, object>();
            this.objectBuilders = new Dictionary<Type, ObjectBuilder>();
            this.keyToImpltypeMap = new Dictionary<TypeRegistration, Type>();
            this.typeToAllMap = new Dictionary<Type, ICollection<TypeRegistration>>();
        }

        #endregion

        #region Public Methods and Operators

        public override bool IsRegistered(Type type, string name)
        {
            return false;
        }

        public override bool IsRegistered(TypeRegistration key)
        {
            return this.RegisteredTypes.Contains(key);
        }

        public override void RegisterInstance(Type type, string name, object instance)
        {
            var typeRegistration = new TypeRegistration(type, name);
            this.RegisterInstance(typeRegistration, instance);
        }

        public override void RegisterInstance(TypeRegistration typeRegistration, object instance)
        {
            if (this.instances.TryAdd(typeRegistration, instance))
            {
                this.typeToAllMap.GetOrAdd(typeRegistration.Type, () => new List<TypeRegistration>())
                    .Add(typeRegistration);
            }
        }

        public override void RegisterType(Type type, string name, Lifecycle lifetime)
        {
            var typeRegistration = new TypeRegistration(type, name);
            this.RegisterType(typeRegistration, lifetime);
        }

        public override void RegisterType(TypeRegistration key, Lifecycle lifetime)
        {
            this.RegisterType(key, key.Type, lifetime);
        }

        public override void RegisterType(Type @from, Type to, string name, Lifecycle lifetime)
        {
            var typeRegistration = new TypeRegistration(@from, name);
            this.RegisterType(typeRegistration, to, lifetime);
        }

        public override void RegisterType(TypeRegistration key, Type implType, Lifecycle lifetime)
        {
            if (this.keyToImpltypeMap.TryAdd(key, implType))
            {
                this.typeToAllMap.GetOrAdd(key.Type, () => new List<TypeRegistration>()).Add(key);
                this.objectBuilders.TryAdd(implType, new ObjectBuilder(implType, lifetime));
            }
        }

        public override object Resolve(Type type, string name)
        {
            var key = new TypeRegistration(type, name);

            return this.Resolve(key);
        }

        public override object Resolve(TypeRegistration typeRegistration)
        {
            object instance = null;
            if (!this.instances.TryGetValue(typeRegistration, out instance))
            {
                Type implType;
                if (this.keyToImpltypeMap.TryGetValue(typeRegistration, out implType))
                {
                    ObjectBuilder objectBuilder;
                    if (this.objectBuilders.TryGetValue(implType, out objectBuilder))
                    {
                        instance = objectBuilder.GetInstance();
                    }
                }
            }

            return instance;
        }

        public override IEnumerable<object> ResolveAll(Type type)
        {
            if (!this.typeToAllMap.ContainsKey(type))
            {
                return Enumerable.Empty<object>();
            }

            return this.typeToAllMap[type].Select(this.Resolve).Distinct().ToArray();
        }

        protected override void Dispose(bool disposing)
        {
        }

        #endregion

        class ObjectBuilder
        {
            #region Fields

            private readonly ConstructorInfo constructorInfo;
            private readonly Lifecycle lifecycle;
            private object instance;

            #endregion

            #region Constructors and Destructors

            public ObjectBuilder(Type type, Lifecycle lifecycle)
            {
                ConstructorInfo[] constructors = type.GetConstructors();
                if (constructors.Length == 0)
                {
                    string errorMessage = string.Format("Type '{0}' must have a public constructor.", type.FullName);
                    throw new SystemException(errorMessage);
                }

                if (constructors.Length > 1)
                {
                    string errorMessage = string.Format(
                        "Type '{0}' must have multiple public constructor.", 
                        type.FullName);
                    throw new SystemException(errorMessage);
                }

                this.constructorInfo = constructors.First();
                this.lifecycle = lifecycle;
            }

            #endregion

            #region Methods and Operators

            public object GetInstance()
            {
                if (this.lifecycle == Lifecycle.Singleton)
                {
                    if (this.instance == null)
                    {
                        this.instance = this.CreateInstance();
                    }

                    return this.instance;
                }

                return this.instance;
            }

            private object CreateInstance()
            {
                ParameterInfo[] parameters = this.constructorInfo.GetParameters();
                if (parameters.Length == 0)
                {
                    return this.constructorInfo.Invoke(new object[0]);
                }

                object[] args = parameters.Select(this.GetParameterValue).ToArray();
                return this.constructorInfo.Invoke(args);
            }

            private object GetParameterValue(ParameterInfo parameterInfo)
            {
                if (parameterInfo.RawDefaultValue == DBNull.Value)
                {
                    return Instance.Resolve(parameterInfo.ParameterType);
                }

                return parameterInfo.RawDefaultValue;
            }

            #endregion
        }
    }
}