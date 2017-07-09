
namespace Umizoo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// <see cref="IEnumerable"/> 的扩展类
    /// </summary>
    public static class EnumerableExtentions
    {
        /// <summary>
        /// 遍历结果集
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if(source.IsEmpty())
                return;

            //foreach(var item in source.GetEnumerator()) {
            //    action(item);
            //}

            for (IEnumerator<T> enumerator = source.GetEnumerator(); enumerator.MoveNext();)
            {
                action(enumerator.Current);
            }
        }

        /// <summary>
        /// 如果 <param name="source" /> 为null，则创建一个空的 <see cref="IEnumerable{T}"/>。
        /// </summary>
        public static IEnumerable<T> Safe<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// 检查 <param name="source" /> 是否为空。
        /// </summary>
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            if(source == null)
                return true;
            var collection = source as ICollection;
            if(collection != null)
                return collection.Count == 0;
            return !source.Any();
        }

        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的集合不能是 null 或 空的集合。
        /// </summary>
        public static void NotEmpty<T>(this IEnumerable<T> source, string variableName)
        {
            if (source.IsEmpty())
                throw new ArgumentNullException(variableName);
        }
    }
}
