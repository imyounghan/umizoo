// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示源数据信息
    /// </summary>
    public class SourceInfo
    {
        /// <summary>
        ///     空的主键
        /// </summary>
        public static readonly SourceInfo Empty = new SourceInfo();

        private SourceInfo()
        {
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public SourceInfo(string str)
        {
            var match = Regex.Match(str, @"^([\w-\.]+)\.([\w-]+),\s?([\w-]+)@([\w-]+)$");
            if (!match.Success) throw new FormatException(str);

            var fullName = string.Format("{0}.{1}, {2}",
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value);
            Type = Type.GetType(fullName);
            Id = match.Groups[4].Value;
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public SourceInfo(object sourceId, Type sourceType)
        {
            Type = sourceType;
            Id = sourceId.ToString();
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public SourceInfo(string sourceId, string sourceNamespace, string sourceTypeName, string sourceAssemblyName)
        {
            Assertions.NotNullOrWhiteSpace(sourceId, "sourceId");
            Assertions.NotNullOrWhiteSpace(sourceNamespace, "sourceNamespace");
            Assertions.NotNullOrWhiteSpace(sourceTypeName, "sourceTypeName");
            Assertions.NotNullOrWhiteSpace(sourceAssemblyName, "sourceAssemblyName");

            var fullName = string.Format("{0}.{1}, {2}", sourceNamespace, sourceTypeName, sourceAssemblyName);
            Type = Type.GetType(fullName);
            Id = sourceId;
        }

        /// <summary>
        ///     程序集名称
        /// </summary>
        public string AssemblyName { get { return Type.GetAssemblyName(); } }

        /// <summary>
        ///     命名空间
        /// </summary>
        public string Namespace { get { return Type.Namespace; } }

        /// <summary>
        ///     类型名称(不包含全名空间)
        /// </summary>
        public string TypeName { get { return Type.GetFriendlyTypeName(); } }

        /// <summary>
        ///     源标识。
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     源标识。
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     输出该结构的字符串格式。
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Namespace))
                sb.Append(Namespace).Append(".");
            if (!string.IsNullOrWhiteSpace(TypeName))
                sb.Append(TypeName);
            if (!string.IsNullOrWhiteSpace(AssemblyName))
                sb.Append(",").Append(AssemblyName);
            if (!string.IsNullOrWhiteSpace(Id))
                sb.Append("@").Append(Id);

            return sb.ToString();
        }

        /// <summary>
        ///     确定此实例是否与指定的对象相同。
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is SourceInfo))
                return false;

            return IsEqual(this, (SourceInfo) obj);
        }

        /// <summary>
        ///     返回此实例的哈希代码。
        /// </summary>
        public override int GetHashCode()
        {
            var codes = new[]
            {
                AssemblyName.ToLowerInvariant().GetHashCode(),
                Namespace.ToLowerInvariant().GetHashCode(),
                TypeName.ToLowerInvariant().GetHashCode(),
                Id.ToLowerInvariant().GetHashCode()
            };
            return codes.Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        ///     获取源类型完整名称但不包括程序集名称。
        /// </summary>
        public string GetSourceTypeName()
        {
            return string.Concat(Namespace, ".", TypeName);
        }

        /// <summary>
        ///     获取源类型完整名称且包括程序集名称。
        /// </summary>
        public string GetSourceTypeFullName()
        {
            return string.Concat(Namespace, ".", TypeName, ", ", AssemblyName);
        }

        /// <summary>
        ///     判断是否相等
        /// </summary>
        public static bool operator ==(SourceInfo left, SourceInfo right)
        {
            return IsEqual(left, right);
        }

        /// <summary>
        ///     判断是否不相等
        /// </summary>
        public static bool operator !=(SourceInfo left, SourceInfo right)
        {
            return !IsEqual(left, right);
        }

        private static bool IsEqual(SourceInfo left, SourceInfo right)
        {
            return string.Equals(left.Id, right.Id, StringComparison.CurrentCultureIgnoreCase)
                   && left.Type == right.Type;
        }


        /// <summary>
        ///     将 <see cref="SourceInfo" /> 的字符串表示形式转换为它的等效的 <see cref="SourceInfo" />。
        /// </summary>
        public static SourceInfo Parse(string input)
        {
            return new SourceInfo(input);
        }

        /// <summary>
        ///     将 <see cref="SourceInfo" /> 的字符串表示形式转换为它的等效的 <see cref="SourceInfo" />。一个指示转换是否成功的返回值。
        /// </summary>
        public static bool TryParse(string input, out SourceInfo result)
        {
            try
            {
                result = Parse(input);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
    }
}