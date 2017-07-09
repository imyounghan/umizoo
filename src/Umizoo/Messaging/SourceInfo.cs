

namespace Umizoo.Messaging
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Umizoo.Infrastructure;

    /// <summary>
    /// 表示一个完整的源数据主键
    /// </summary>
    public class SourceInfo
    {
        /// <summary>
        /// 空的主键
        /// </summary>
        public static readonly SourceInfo Empty = new SourceInfo();

        private SourceInfo()
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public SourceInfo(string str)
        {
            var match = Regex.Match(str, @"^([\w-\.]+)\.([\w-]+),\s?([\w-]+)@([\w-]+)$");
            if(!match.Success) {
                throw new FormatException(str);
            }

            string fullName = string.Format("{0}.{1}, {2}",
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value);
            this.sourceType = Type.GetType(fullName);
            this.sourceId = match.Groups[4].Value;
        }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public SourceInfo(object sourceId, Type sourceType)
        {
            this.sourceType = sourceType;
            this.sourceId = sourceId.ToString();
        }
        
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public SourceInfo(string sourceId, string sourceNamespace, string sourceTypeName, string sourceAssemblyName)
        {
            Ensure.NotNullOrWhiteSpace(sourceId, "sourceId");
            Ensure.NotNullOrWhiteSpace(sourceNamespace, "sourceNamespace");
            Ensure.NotNullOrWhiteSpace(sourceTypeName, "sourceTypeName");
            Ensure.NotNullOrWhiteSpace(sourceAssemblyName, "sourceAssemblyName");

            string fullName = string.Format("{0}.{1}, {2}",
                sourceNamespace,
                sourceTypeName,
                sourceAssemblyName);
            this.sourceType = Type.GetType(fullName);
            this.sourceId = sourceId;
        }

        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return this.sourceType.GetAssemblyName();
            }
        }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace
        {
            get { return this.sourceType.Namespace; }
        }

        /// <summary>
        /// 类型名称(不包含全名空间)
        /// </summary>
        public string TypeName
        {
            get { return this.sourceType.Name; }
        }
        
        private string sourceId;
        /// <summary>
        /// 源标识。
        /// </summary>
        public string Id
        {
            get { return this.sourceId; }
            private set { this.sourceId = value; }
        }

        private Type sourceType;
        /// <summary>
        /// 源标识。
        /// </summary>
        public Type Type
        {
            get { return this.sourceType; }
            private set { this.sourceType = value; }
        }

        /// <summary>
        /// 输出该结构的字符串格式。
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(!string.IsNullOrWhiteSpace(this.Namespace))
                sb.Append(this.Namespace).Append(".");
            if(!string.IsNullOrWhiteSpace(this.TypeName))
                sb.Append(this.TypeName);
            if(!string.IsNullOrWhiteSpace(this.AssemblyName))
                sb.Append(",").Append(this.AssemblyName);
            if(!string.IsNullOrWhiteSpace(this.Id))
                sb.Append("@").Append(this.Id);

            return sb.ToString();
        }

        /// <summary>
        /// 确定此实例是否与指定的对象相同。
        /// </summary>
        public override bool Equals(object obj)
        {
            if(!(obj is SourceInfo))
                return false;
            
            return IsEqual(this, (SourceInfo)obj);
        }

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        public override int GetHashCode()
        {
            var codes = new int[] {
                this.AssemblyName.ToLowerInvariant().GetHashCode(),
                this.Namespace.ToLowerInvariant().GetHashCode(),
                this.TypeName.ToLowerInvariant().GetHashCode(),
                this.Id.ToLowerInvariant().GetHashCode()
            };
            return codes.Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// 获取源类型完整名称但不包括程序集名称。
        /// </summary>
        public string GetSourceTypeName()
        {
            return string.Concat(this.Namespace, ".", this.TypeName);
        }
        /// <summary>
        /// 获取源类型完整名称且包括程序集名称。
        /// </summary>
        public string GetSourceTypeFullName()
        {
            return string.Concat(this.Namespace, ".", this.TypeName, ", ", this.AssemblyName);
        }

        /// <summary>
        /// 判断是否相等
        /// </summary>
        public static bool operator ==(SourceInfo left, SourceInfo right)
        {
            return IsEqual(left, right);
        }
        /// <summary>
        /// 判断是否不相等
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
        /// 将 <see cref="SourceKey"/> 的字符串表示形式转换为它的等效的 <see cref="SourceKey"/>。
        /// </summary>
        public static SourceInfo Parse(string input)
        {
            return new SourceInfo(input);
        }

        /// <summary>
        /// 将 <see cref="SourceKey"/> 的字符串表示形式转换为它的等效的 <see cref="SourceKey"/>。一个指示转换是否成功的返回值。
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
