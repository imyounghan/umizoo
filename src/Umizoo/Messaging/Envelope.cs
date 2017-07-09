
namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Generic;

    public class Envelope : EventArgs
    {

    }

    /// <summary>
    /// 提供封装一个对象的信封
    /// </summary>
    public class Envelope<T> : EventArgs
    {
        public Envelope()
        {
        }

        /// <summary>
        /// 初始化一个 <see cref="Envelope{T}"/> 类的新实例
        /// </summary>
        public Envelope(T body)
            : this(body, (string)null)
        {
        }

        public Envelope(T body, string messageId)
        {
            this.Body = body;
            this.MessageId = messageId;
            this.Items = new Dictionary<string, object>();
        }

        /// <summary>
        /// 获取该信封的主体对象
        /// </summary>
        public T Body { get; private set; }

        /// <summary>
        /// 获取消息的Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 键值对的集合
        /// </summary>
        public IDictionary<string, object> Items { get; set; }

        public override string ToString()
        {
            return string.Concat(this.Body, "#", this.MessageId);
        }
    }
}
