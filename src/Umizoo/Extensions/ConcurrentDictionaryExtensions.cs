
namespace Umizoo
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// ConcurrentDictionary的扩展
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// 删除存在key的元素
        /// </summary>
        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryRemove(key);
        }

        /// <summary>
        /// 删除存在key的元素
        /// </summary>
        public static TValue RemoveAndGet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null || dict.Count == 0)
                return default(TValue);

            TValue value;
            if (dict.TryRemove(key, out value)) {
                return value;
            }

            return default(TValue);
        }

        /// <summary>
        /// 删除存在key的元素
        /// </summary>
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null || dict.Count == 0)
                return false;

            TValue value;
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        /// 获取key的元素
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }

            return default(TValue);
        }

        /// <summary>
        /// 获取key的元素
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            TValue originalValue;
            if (dict.TryGetValue(key, out originalValue)) {
                return originalValue;
            }

            return value;
        }

        /// <summary>
        /// 获取key的元素
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }

            return valueFactory.Invoke();
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到字典中。
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value)) {
                value = valueFactory.Invoke();
                dict.TryAdd(key, value);
            }

            return value;
        }
    }
}
