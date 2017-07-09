

namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web.Script.Serialization;

    using Umizoo.Infrastructure;

    [DataContract]
    public class PublishableExceptionDescriptor : IDescriptor
    {
        [DataContract]
        public class PropertyEntry
        {
            public PropertyEntry()
            { }

            public PropertyEntry(Type type)
            {
                this.Namespace = type.Namespace;
                this.TypeName = type.Name;
                this.AssemblyName = type.GetAssemblyName();
            }

            /// <summary>
            /// 程序集
            /// </summary>
            [DataMember(Name = "assembly")]
            public string AssemblyName { get; set; }
            /// <summary>
            /// 命名空间
            /// </summary>
            [DataMember(Name = "namespace")]
            public string Namespace { get; set; }
            /// <summary>
            /// 类型名称
            /// </summary>
            [DataMember(Name = "type")]
            public string TypeName { get; set; }


            [DataMember(Name = "key")]
            public string Key { get; set; }

            [DataMember(Name = "value")]
            public string Value { get; set; }

            public string GetMetadataTypeName()
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(this.Namespace))
                    sb.Append(this.Namespace).Append(".");
                if (!string.IsNullOrWhiteSpace(this.TypeName))
                    sb.Append(this.TypeName);
                if (!string.IsNullOrWhiteSpace(this.AssemblyName))
                    sb.Append(", ").Append(this.AssemblyName);

                return sb.ToString();
            }
        }


        public PublishableExceptionDescriptor()
        { }

        public PublishableExceptionDescriptor(IPublishableException exception)
        {
            var keyProvider = exception as IRoutingProvider;
            if (keyProvider != null)
            {
                this.Key = keyProvider.GetRoutingKey();
            }
            else
            {
                this.Key = string.Empty;
            }
        }

        [IgnoreDataMember]
        [ScriptIgnore]
        public string Key { get; set; }

        [DataMember(Name = "id")]
        public string ExceptionId { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        [DataMember(Name = "typeName")]
        public string TypeName { get; set; }

        [DataMember(Name = "items")]
        public IEnumerable<PropertyEntry> Items { get; set; }

        public string GetKey()
        {
            return this.Key;
        }

        public override string ToString()
        {
            var array = Items.Select(item => string.Concat(item.Key, ":", item.Value));

            return string.Format("{0}({1})", TypeName, string.Join(", ", array));
        }
    }
}
