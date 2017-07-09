

namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;


    [DataContract]
    public class QueryDescriptor : IDescriptor
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        [DataMember(Name = "typeName")]
        public string TypeName { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        [DataMember(Name = "metadata")]
        public string Metadata { get; set; }

        [DataMember(Name = "traceId")]
        public string TraceId { get; set; }

        [DataMember(Name = "traceAddress")]
        public string TraceAddress { get; set; }


        public virtual string GetKey()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})#{2}@{3}", TypeName, Metadata, TraceId, TraceAddress);
        }
    }
}
