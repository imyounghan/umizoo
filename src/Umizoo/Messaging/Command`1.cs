

namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;

    /// <summary>
    /// 表示创建或修改一个聚合根的命令
    /// </summary>
    [DataContract]
    public abstract class Command<TAggregateRootId> : Command
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        protected Command()
        { }

        /// <summary>
        /// 聚合根ID
        /// </summary>
        [DataMember(Name = "aggregateRootId")]
        public TAggregateRootId AggregateRootId { get; set; }

        protected override string GetRoutingKey()
        {
            if(AggregateRootId != null) {
                return AggregateRootId.ToString();
            }

            return null;
        }

        /// <summary>
        /// 输出字符串信息
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.GetType().FullName, "#", this.AggregateRootId);
        }

    }
}
