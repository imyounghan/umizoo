
namespace Umizoo.Messaging
{
    using System;
    using System.Runtime.Serialization;
    using Umizoo.Infrastructure;

    /// <summary>
    /// 表示继承该抽象类的是一个事件
    /// </summary>
    [DataContract]
    public abstract class Event : MessageBase, IEvent
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        protected Event()
        {
            this.CreationTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember(Name = "creationTime")]
        public DateTime CreationTime { get; set; }
    }
}
