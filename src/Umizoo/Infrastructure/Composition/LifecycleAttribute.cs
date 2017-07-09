
namespace Umizoo.Infrastructure.Composition
{
    using System;

    /// <summary>
    /// 表示实例的生命周期的特性(默认为Singleton)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LifecycleAttribute : Attribute
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LifecycleAttribute()
            : this(Lifecycle.Singleton)
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public LifecycleAttribute(Lifecycle lifecycle)
        {
            this.Lifecycle = lifecycle;
        }

        /// <summary>
        /// 返回生命周期类型(默认为Singleton)
        /// </summary>
        public Lifecycle Lifecycle { get; private set; }

        /// <summary>
        /// 获取生命周期
        /// </summary>
        public static Lifecycle GetLifecycle(Type type)
        {
            var attribute = type.GetSingleAttribute<LifecycleAttribute>(false);

            if(attribute == null)
                return Lifecycle.Singleton;

            return attribute.Lifecycle;
        }
    }
}
