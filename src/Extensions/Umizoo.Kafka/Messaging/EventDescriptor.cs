
namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;

    [DataContract]
    public class EventDescriptor : QueryDescriptor
    {

        [DataMember(Name = "Id")]
        public string EventId { get; set; }


        [DataMember(Name = "commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// 源类型名称
        /// </summary>
        [DataMember(Name = "commandTypeName", EmitDefaultValue = false)]
        public string CommandTypeName { get; set; }

        /// <summary>
        /// 源类型名称
        /// </summary>
        [DataMember(Name = "sourceTypeName", EmitDefaultValue = false)]
        public string SourceTypeName { get; set; }

        [DataMember(Name = "sourceId", EmitDefaultValue = false)]
        public string SourceId { get; set; }


        public override string GetKey()
        {
            return this.SourceId;
        }
    }
}
