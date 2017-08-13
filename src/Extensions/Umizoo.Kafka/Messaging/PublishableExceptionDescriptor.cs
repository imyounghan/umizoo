using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    [DataContract]
    public class PublishableExceptionDescriptor : IDescriptor
    {
        public PublishableExceptionDescriptor()
        {
        }

        public PublishableExceptionDescriptor(IPublishableException exception)
        {
            var keyProvider = exception as IRoutingProvider;
            if (keyProvider != null)
                Key = keyProvider.GetRoutingKey();
            else
                Key = string.Empty;
        }

        [IgnoreDataMember]
        [ScriptIgnore]
        public string Key { get; set; }

        [DataMember(Name = "id")]
        public string ExceptionId { get; set; }

        /// <summary>
        ///     类型名称
        /// </summary>
        [DataMember(Name = "typeName")]
        public string TypeName { get; set; }

        [DataMember(Name = "items")]
        public IEnumerable<PropertyEntry> Items { get; set; }

        public string GetKey()
        {
            return Key;
        }

        public override string ToString()
        {
            var array = Items.Select(item => string.Concat(item.Key, ":", item.Value));

            return string.Format("{0}({1})", TypeName, string.Join(", ", array));
        }

        [DataContract]
        public class PropertyEntry
        {
            public PropertyEntry()
            {
            }

            public PropertyEntry(Type type)
            {
                Namespace = type.Namespace;
                TypeName = type.Name;
                AssemblyName = type.GetAssemblyName();
            }

            /// <summary>
            ///     程序集
            /// </summary>
            [DataMember(Name = "assembly")]
            public string AssemblyName { get; set; }

            /// <summary>
            ///     命名空间
            /// </summary>
            [DataMember(Name = "namespace")]
            public string Namespace { get; set; }

            /// <summary>
            ///     类型名称
            /// </summary>
            [DataMember(Name = "type")]
            public string TypeName { get; set; }


            [DataMember(Name = "key")]
            public string Key { get; set; }

            [DataMember(Name = "value")]
            public string Value { get; set; }

            public string GetMetadataTypeName()
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Namespace))
                    sb.Append(Namespace).Append(".");
                if (!string.IsNullOrWhiteSpace(TypeName))
                    sb.Append(TypeName);
                if (!string.IsNullOrWhiteSpace(AssemblyName))
                    sb.Append(", ").Append(AssemblyName);

                return sb.ToString();
            }
        }
    }
}