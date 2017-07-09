

namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;

    [DataContract]
    public abstract class HandleResult : MessageBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HandleResult()
            : this(HandleStatus.Success)
        { }

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public HandleResult(HandleStatus status, string errorMessage = null)
        {
            this.Status = status;
            this.ErrorMessage = errorMessage;
            
            this.ReplyServer = string.Concat(ConfigurationSettings.InnerAddress, 
                "@", ConfigurationSettings.ServiceName);
        }

        /// <summary>
        /// 失败的消息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 返回处理状态。
        /// </summary>
        [DataMember]
        public HandleStatus Status { get; set; }

        [DataMember]
        public string ReplyServer { get; set; }
    }
}
