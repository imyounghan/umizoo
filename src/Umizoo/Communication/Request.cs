
namespace Umizoo.Communication
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// 请求数据
    /// </summary>
    [DataContract]
    public class Request
    {
        /// <summary>
        /// 包括的头信息
        /// </summary>
        [DataMember(Name = "header", EmitDefaultValue = false)]
        public IDictionary<string, string> Header { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        [DataMember(Name = "body")]
        public string Body { get; set; }
    }
}
