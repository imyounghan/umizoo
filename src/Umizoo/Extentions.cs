// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Umizoo
{
    public static class Extentions
    {
        #region

        /// <summary>
        ///     删除存在key的元素
        /// </summary>
        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryRemove(key);
        }

        /// <summary>
        ///     删除存在key的元素
        /// </summary>
        public static TValue RemoveAndGet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null || dict.Count == 0)
                return default(TValue);

            TValue value;
            if (dict.TryRemove(key, out value)) return value;

            return default(TValue);
        }

        /// <summary>
        ///     删除存在key的元素
        /// </summary>
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null || dict.Count == 0)
                return false;

            TValue value;
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        ///     获取key的元素
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key,
            TValue value = default(TValue))
        {
            TValue originalValue;
            if (dict.TryGetValue(key, out originalValue)) return originalValue;

            return value;
        }

        /// <summary>
        ///     获取key的元素
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key,
            Func<TValue> valueFactory)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) return value;

            return valueFactory.Invoke();
        }

        /// <summary>
        ///     如果指定的键尚不存在，则将键/值对添加到字典中。
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key,
            Func<TValue> valueFactory)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
            {
                value = valueFactory.Invoke();
                dict.TryAdd(key, value);
            }

            return value;
        }

        #endregion

        #region

        /// <summary>
        ///     如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey,TValue}" /> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}" /> 中的键/值对。
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
        ///     如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}" /> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}" /> 中的键/值对。
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
        ///     如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}" /> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}" /> 中的键/值对。
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
        ///     如果指定的键尚不存在，则将键/值对添加到 <see cref="Dictionary{TKey, TValue}" /> 中；如果指定的键已存在，则更新
        ///     <see cref="Dictionary{TKey, TValue}" /> 中的键/值对。
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
        ///     如果指定的键尚不存在，则将键/值对添加到字典中。
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
        ///     如果指定的键尚不存在，则将键/值对添加到字典中。
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
        ///     尝试将指定的键和值添加到字典中。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="value">键对应的值</param>
        /// <returns>如果添加成功返回true，否则为false。</returns>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key)) return false;

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
        ///     尝试从字典中移除并返回具有指定键的值。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">键和值的集合</param>
        /// <param name="key">键值</param>
        /// <param name="value">键对应的值</param>
        /// <returns>如果删除成功返回true，否则为false。</returns>
        public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (dict.TryGetValue(key, out value)) return dict.Remove(key);

            return false;
        }

        public static TValue GetIfKeyNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            TValue defaultValue = default(TValue))
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) return value;

            return defaultValue;
        }

        public static TValue GetIfKeyNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            Func<TValue> valueFactory)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) return value;

            return valueFactory();
        }

        public static TValue GetIfKeyNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (dict.TryGetValue(key, out value)) return value;

            return valueFactory(key);
        }

        #endregion

        #region

        /// <summary>
        ///     遍历结果集
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source.IsEmpty())
                return;


            for (var enumerator = source.GetEnumerator(); enumerator.MoveNext();) action(enumerator.Current);
        }

        /// <summary>
        ///     如果
        ///     <param name="source" />
        ///     为null，则创建一个空的 <see cref="IEnumerable{T}" />。
        /// </summary>
        public static IEnumerable<T> Safe<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        /// <summary>
        ///     检查
        ///     <param name="source" />
        ///     是否为空。
        /// </summary>
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true;
            var collection = source as ICollection;
            if (collection != null)
                return collection.Count == 0;
            return !source.Any();
        }

        /// <summary>
        ///     参数名称为 <paramref name="variableName" /> 的集合不能是 null 或 空的集合。
        /// </summary>
        public static void NotEmpty<T>(this IEnumerable<T> source, string variableName)
        {
            if (source.IsEmpty())
                throw new ArgumentNullException(variableName);
        }

        #endregion

        #region

        public static TResult WaitResult<TResult>(this Task<TResult> task, int millisecondsTimeout,
            TResult defautValue = default(TResult))
        {
            if (millisecondsTimeout > 0)
            {
                if (task.Wait(millisecondsTimeout)) return task.Result;
                return defautValue;
            }

            return task.Result;
        }

        public static TResult WaitResult<TResult>(this Task<TResult> task, int millisecondsTimeout,
            Func<TResult> defaultValueFactory)
        {
            if (millisecondsTimeout > 0)
            {
                if (task.Wait(millisecondsTimeout)) return task.Result;
                return defaultValueFactory.Invoke();
            }

            return task.Result;
        }

        #endregion

        #region

        /// <summary>
        ///     表示
        ///     <param name="type" />
        ///     是否可为 null.
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        ///     表示
        ///     <param name="type" />
        ///     是否可以分配 null.
        /// </summary>
        public static bool CanBeNull(this Type type)
        {
            return IsNullable(type) || !type.IsValueType;
        }

        /// <summary>
        ///     获取指定可以为 null 的类型的基础类型参数
        /// </summary>
        public static Type GetNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        /// <summary>
        ///     获取该类型的完整名称且包括程序集名称
        /// </summary>
        public static string GetFullName(this Type type)
        {
            return string.Concat(type.FullName, ", ", type.GetAssemblyName());
        }

        /// <summary>
        ///     Returns type name without generic specification
        /// </summary>
        public static string GetShortName(this Type t)
        {
            var name = t.Name;
            if (t.IsGenericTypeDefinition)
                return name.Split('`')[0];
            return name;
        }

        /// <summary>
        ///     获取类型的友好名称，主要针对泛型
        /// </summary>
        public static string GetFriendlyTypeName(this Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var sb = new StringBuilder();
            sb.AppendFormat("{0}<", Regex.Replace(type.Name, @"\`\d+$", string.Empty));
            foreach (var typeParameter in type.GetGenericArguments())
                sb.AppendFormat("{0}, ", GetFriendlyTypeName(typeParameter));
            sb.Remove(sb.Length - 2, 2);
            sb.Append(">");
            return sb.ToString();
        }

        /// <summary>
        ///     获取该类型的默认值
        /// </summary>
        public static object GetDefaultValue(this Type type)
        {
            if (!type.IsValueType)
                return null;
            return Activator.CreateInstance(type);
        }

        /// <summary>
        ///     获取该类型的程序集名称
        /// </summary>
        public static string GetAssemblyName(this Type type)
        {
            return Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.FullyQualifiedName);
        }

        /// <summary>
        ///     获取该成员上的全部特性
        /// </summary>
        public static IEnumerable<TAttribute> GetAllAttributes<TAttribute>(this ICustomAttributeProvider target,
            bool inherit)
            where TAttribute : Attribute
        {
            return target.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        /// <summary>
        ///     获取该成员上的其中一个特性
        /// </summary>
        public static TAttribute GetSingleAttribute<TAttribute>(this ICustomAttributeProvider target, bool inherit)
            where TAttribute : Attribute
        {
            return target.GetAllAttributes<TAttribute>(inherit).FirstOrDefault();
        }

        #endregion

        #region

        /// <summary>
        ///     验证模型的正确性
        /// </summary>
        public static bool IsValid<TModel>(this TModel model, out IEnumerable<ValidationResult> errors)
            where TModel : class
        {
            errors = from property in TypeDescriptor.GetProperties(model).Cast<PropertyDescriptor>()
                from attribute in property.Attributes.OfType<ValidationAttribute>()
                where !attribute.IsValid(property.GetValue(model))
                select new ValidationResult(attribute.FormatErrorMessage(property.DisplayName ?? property.Name));

            return !errors.Any();
        }

        /// <summary>
        ///     检查当前对象是否为 null
        /// </summary>
        public static bool IsNull(this object obj)
        {
            return obj == null || obj == DBNull.Value;
        }

        /// <summary>
        ///     如果当前的字符串不为空，则返回加前缀后的字符串
        /// </summary>
        public static string BeforeContact(this string str, string prefix)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : string.Concat(prefix, str);
        }

        /// <summary>
        ///     如果当前的字符串不为空，则返回加后缀后的字符串
        /// </summary>
        public static string AfterContact(this string str, string suffix)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : string.Concat(str, suffix);
        }

        /// <summary>
        ///     分割字符串
        /// </summary>
        public static string[] Split(this string str, string split)
        {
            return (from piece in Regex.Split(str, Regex.Escape(split), RegexOptions.IgnoreCase)
                let trimmed = piece.Trim()
                where !string.IsNullOrEmpty(trimmed)
                select trimmed).ToArray();
        }

        /// <summary>
        ///     判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="str">要查找的字符串</param>
        /// <param name="array">字符串数组</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为不区分, false为区分</param>
        public static bool InArray(this string str, string[] array, bool caseInsensetive = true)
        {
            return Array.Exists(array, delegate(string element)
            {
                return string.Equals(element, str,
                    caseInsensetive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            });
        }

        /// <summary>
        ///     判断指定字符串在指定字符串数组中的位置
        /// </summary>
        /// <param name="str">要查找的字符串</param>
        /// <param name="array">字符串数组</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为不区分, false为区分</param>
        public static int InArrayIndexOf(this string str, string[] array, bool caseInsensetive = true)
        {
            return Array.FindIndex(array, delegate(string element)
            {
                return string.Equals(element, str,
                    caseInsensetive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            });
        }

        /// <summary>
        ///     判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="strarray">内部以逗号分割单词的字符串</param>
        /// <param name="strsplit">分割字符串</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为不区分, false为区分</param>
        /// <returns>判断结果</returns>
        public static bool InArray(this string str, string strarray, string strsplit = ",",
            bool caseInsensetive = false)
        {
            return Array.Exists(Split(strarray, strsplit), delegate(string element)
            {
                return string.Equals(element, str,
                    caseInsensetive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            });
        }

        /// <summary>
        ///     判定字符串是不是数值型
        /// </summary>
        public static bool IsNumeric(this string digit)
        {
            return Regex.IsMatch(digit, @"^[-]?[0-9]*$");
        }

        /// <summary>
        ///     判断字符串是不是日期格式
        /// </summary>
        public static bool IsDate(this string date)
        {
            return Regex.IsMatch(date, @"^(\d{4})(-|/|\.)(\d{1,2})(-|/|\.)(\d{1,2})$");
        }

        /// <summary>
        ///     判断字符串是不是时间格式
        /// </summary>
        public static bool IsTime(this string time)
        {
            return Regex.IsMatch(time, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        /// <summary>
        ///     判断字符串是不是日期时间模式
        /// </summary>
        public static bool IsDateTime(this string dateTime)
        {
            return Regex.IsMatch(dateTime,
                @"(\d{4})(-|/|\.)(\d{1,2})(-|/|\.)(\d{1,2}) ^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        /// <summary>
        ///     判断字符串是不是小数模式
        /// </summary>
        public static bool IsDecimal(this string @decimal)
        {
            return Regex.IsMatch(@decimal, @"^[-]?[0-9]*[.]?[0-9]*$");
        }

        /// <summary>
        ///     如果当前的对象满足条件返回默认值，否则返回源值
        /// </summary>
        public static T If<T>(this T source, Func<T, bool> conditionFunc, T defaultValue = default(T))
        {
            return conditionFunc(source) ? defaultValue : source;
        }

        /// <summary>
        ///     如果当前的对象满足条件返回默认值，否则返回源值
        /// </summary>
        public static T If<T>(this T source, Func<T, bool> conditionFunc, Func<T> valueFactory)
        {
            return conditionFunc(source) ? valueFactory() : source;
        }

        /// <summary>
        ///     如果当前的字符串为空，则返回安全值
        /// </summary>
        public static string IfEmpty(this string str, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(str) ? defaultValue : str;
        }

        /// <summary>
        ///     如果当前的字符串为空，则返回安全值
        /// </summary>
        public static string IfEmpty(this string str, Func<string> valueFactory)
        {
            return string.IsNullOrWhiteSpace(str) ? valueFactory() : str;
        }


        /// <summary>
        ///     将
        ///     <param name="str" />
        ///     转换为
        ///     <param name="targetType" />
        ///     的值。转换失败会抛异常
        /// </summary>
        public static object Change(this string str, Type targetType)
        {
            if (targetType.IsValueType)
            {
                if (typeof(bool) == targetType)
                {
                    var lb = str.ToUpper();
                    if (lb == "T" || lb == "F" || lb == "TRUE" || lb == "FALSE" ||
                        lb == "Y" || lb == "N" || lb == "YES" ||
                        lb == "NO") return lb == "T" || lb == "TRUE" || lb == "Y" || lb == "YES";
                }

                var method = targetType.GetMethod("Parse", new[] {typeof(string)});
                if (method != null) return method.Invoke(null, new object[] {str});
            }

            if (targetType.IsEnum) return Enum.Parse(targetType, str);


            throw new ArgumentException(string.Format("Unhandled type of '{0}'.", targetType));
        }

        /// <summary>
        ///     将
        ///     <param name="str" />
        ///     转换为
        ///     <param name="targetType" />
        ///     的值。如果转换失败则使用
        ///     <param name="defaultValue" />
        ///     的值。
        /// </summary>
        public static object ChangeIfError(this string str, Type targetType, object defaultValue)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                return str.Change(targetType);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        ///     将
        ///     <param name="str" />
        ///     转换为
        ///     <param name="targetType" />
        ///     的值。一个指示转换是否成功的返回值
        ///     <param name="result" />
        ///     。
        /// </summary>
        public static bool TryChange(this string str, Type targetType, out object result)
        {
            try
            {
                result = str.Change(targetType);
                return true;
            }
            catch
            {
                result = targetType.GetDefaultValue();
                return false;
            }
        }


        /// <summary>
        ///     将
        ///     <param name="str" />
        ///     转换为
        ///     <typeparam name="T" />
        ///     的值。
        /// </summary>
        public static T Change<T>(this string str)
            where T : struct
        {
            return (T) Change(str, typeof(T));
        }

        /// <summary>
        ///     将
        ///     <param name="str" />
        ///     转换为
        ///     <typeparam name="T" />
        ///     的值。如果转换失败则使用
        ///     <param name="defaultValue" />
        ///     的值。
        /// </summary>
        public static T ChangeIfError<T>(this string str, T defaultValue = default(T))
            where T : struct
        {
            T result;
            if (!string.IsNullOrEmpty(str) && str.TryChange(out result)) return result;

            return defaultValue;
        }

        /// <summary>
        ///     将
        ///     <param name="str" />
        ///     转换为
        ///     <typeparam name="T" />
        ///     的值。一个指示转换是否成功的返回值
        ///     <param name="result" />
        ///     。
        /// </summary>
        public static bool TryChange<T>(this string str, out T result)
            where T : struct
        {
            if (string.IsNullOrEmpty(str))
            {
                result = default(T);
                return false;
            }

            try
            {
                result = str.Change<T>();
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public static TRole ActAs<TRole>(this object obj) where TRole : class
        {
            if (!typeof(TRole).IsInterface)
                throw new ApplicationException(
                    string.Format("'{0}' is not an interface type.", typeof(TRole).FullName));

            var actor = obj as TRole;

            if (actor == null)
                throw new ApplicationException(string.Format("'{0}' cannot act as role '{1}'.", obj.GetType().FullName,
                    typeof(TRole).FullName));

            return actor;
        }

        #endregion
    }
}