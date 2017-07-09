
namespace Umizoo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// <see cref="Type"/> 的扩展类
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 表示 <param name="type" /> 是否可为 null.
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 表示 <param name="type" /> 是否可以分配 null.
        /// </summary>
        public static bool CanBeNull(this Type type)
        {
            return IsNullable(type) || !type.IsValueType;
        }

        /// <summary>
        /// 获取指定可以为 null 的类型的基础类型参数
        /// </summary>
        public static Type GetNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        /// <summary>
        /// 获取该类型的完整名称且包括程序集名称
        /// </summary>
        public static string GetFullName(this Type type)
        {
            return string.Concat(type.FullName, ", ", type.GetAssemblyName());
        }

        /// <summary>
        /// Returns type name without generic specification
        /// </summary>
        public static string GetShortName(this Type t)
        {
            var name = t.Name;
            if(t.IsGenericTypeDefinition)
                return name.Split('`')[0];
            return name;
        }

        /// <summary>
        /// 获取该类型的默认值
        /// </summary>
        public static object GetDefaultValue(this Type type)
        {
            if(!type.IsValueType)
                return null;
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 获取该类型的程序集名称
        /// </summary>
        public static string GetAssemblyName(this Type type)
        {
            return Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.FullyQualifiedName);
        }

        /// <summary>
        /// 获取该成员上的全部特性
        /// </summary>
        public static IEnumerable<TAttribute> GetAllAttributes<TAttribute>(this ICustomAttributeProvider target, bool inherit)
           where TAttribute : Attribute
        {
            return target.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        /// <summary>
        /// 获取该成员上的其中一个特性
        /// </summary>
        public static TAttribute GetSingleAttribute<TAttribute>(this ICustomAttributeProvider target, bool inherit)
           where TAttribute : Attribute
        {
            return target.GetAllAttributes<TAttribute>(inherit).FirstOrDefault();
        }
    }
}
