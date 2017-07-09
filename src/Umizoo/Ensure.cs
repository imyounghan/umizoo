using System;
using System.Collections.Generic;

namespace Umizoo
{
    public static class Ensure
    {
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的值不能是 null。
        /// </summary>
        public static void NotNull(object obj, string variableName)
        {
            if (obj.IsNull())
                throw new ArgumentNullException(variableName);
        }

        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的字符串不能 <see cref="string.Empty"/> 字符串。
        /// </summary>
        public static void NotEmpty(string @string, string variableName)
        {
            if (@string != null && string.IsNullOrEmpty(@string))
                throw new ArgumentException(variableName);
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的字符串不能是 null 或 <see cref="string.Empty"/> 字符串。
        /// </summary>
        public static void NotNullOrEmpty(string @string, string variableName)
        {
            if (string.IsNullOrEmpty(@string))
                throw new ArgumentNullException(variableName);
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的字符串不能是空白字符串。
        /// </summary>
        public static void NotWhiteSpace(string @string, string variableName)
        {
            if (@string != null && string.IsNullOrWhiteSpace(@string))
                throw new ArgumentException(variableName);
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的字符串不能是 null 或 空白字符串。
        /// </summary>
        public static void NotNullOrWhiteSpace(string @string, string variableName)
        {
            if (string.IsNullOrWhiteSpace(@string))
                throw new ArgumentNullException(variableName);
        }

        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的数值必须是正整数。
        /// </summary>
        public static void MustPositive(int number, string variableName)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be positive."));
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的数值必须是正整数。
        /// </summary>
        public static void MustPositive(long number, string variableName)
        {
            if (number <= 0L)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be positive."));
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的数值必须是正整数。
        /// </summary>
        public static void MustPositive(this decimal number, string variableName)
        {
            if (number <= 0m)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be positive."));
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的数值不能是负数。
        /// </summary>
        public static void NonNegative(long number, string variableName)
        {
            if (number < 0L)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be non negative."));
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的数值不能是负数。
        /// </summary>
        public static void NonNegative(int number, string variableName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be non negative."));
        }
        /// <summary>
        /// 参数名称为 <paramref name="variableName"/> 的数值不能是负数。
        /// </summary>
        public static void NonNegative(decimal number, string variableName)
        {
            if (number < 0m)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be non negative."));
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
