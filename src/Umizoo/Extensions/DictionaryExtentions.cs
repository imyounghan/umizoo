
namespace Umizoo
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     <see cref="Dictionary{TKey,TValue}" /> 的扩展类
    /// </summary>
    public static class DictionaryExtentions
    {

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}"/> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}"/> 中的键/值对。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="addValueFactory">添加键值的工厂方法</param>
        /// <param name="updateValueFactory">更新键值的工厂方法</param>
        /// <returns>返回最终的值</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            Func<TKey, TValue> addValueFactory, 
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;

            if (dict.TryGetValue(key, out value))
            {
                value = updateValueFactory.Invoke(key, value);
                dict[key] = value;
            }
            else
            {
                value = addValueFactory.Invoke(key);
                dict.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}"/> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}"/> 中的键/值对。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="addValue">添加键对应的值</param>
        /// <param name="updateValueFactory">更新键值的工厂方法</param>
        /// <returns>返回最终的值</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            TValue addValue, 
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            return dict.AddOrUpdate(key, k => addValue, updateValueFactory);
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}"/> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}"/> 中的键/值对。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="addValueFactory">添加键值的工厂方法</param>
        /// <param name="updateValueFactory">更新键值的工厂方法</param>
        /// <returns>返回最终的值</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            Func<TValue> addValueFactory, 
            Func<TValue, TValue> updateValueFactory)
        {
            TValue value;

            if (dict.TryGetValue(key, out value))
            {
                value = updateValueFactory.Invoke(value);
                dict[key] = value;
            }
            else
            {
                value = addValueFactory.Invoke();
                dict.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}"/> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}"/> 中的键/值对。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="addValue">添加键对应的值</param>
        /// <param name="updateValueFactory">更新键值的工厂方法</param>
        /// <returns>返回最终的值</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            TValue addValue, 
            Func<TValue, TValue> updateValueFactory)
        {
            return dict.AddOrUpdate(key, () => addValue, updateValueFactory);
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到字典中。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="valueFactory">添加键值的工厂方法</param>
        /// <returns>返回键对应的值</returns>
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
            {
                value = valueFactory.Invoke(key);
                dict.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到字典中。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="valueFactory">添加键值的工厂方法</param>
        /// <returns>返回键对应的值</returns>
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            Func<TValue> valueFactory)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
            {
                value = valueFactory.Invoke();
                dict.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// 尝试将指定的键和值添加到字典中。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="value">键对应的值</param>
        /// <returns>如果添加成功返回true，否则为false。</returns>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                return false;
            }

            try
            {
                dict.Add(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试从字典中移除并返回具有指定键的值。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="value">键对应的值</param>
        /// <returns>如果删除成功返回true，否则为false。</returns>
        public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (dict.TryGetValue(key, out value))
            {
                return dict.Remove(key);
            }

            return false;
        }

        public static TValue GetIfKeyNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            if(dict.TryGetValue(key, out value)) {
                return value;
            }

            return defaultValue;
        }

        public static TValue GetIfKeyNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }

            return valueFactory();
        }

        public static TValue GetIfKeyNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }

            return valueFactory(key);
        }
    }
}