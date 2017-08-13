// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Runtime.Serialization;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示创建或修改一个聚合根的命令
    /// </summary>
    [DataContract]
    public abstract class Command<TAggregateRootId> : Command
    {
        /// <summary>
        ///     聚合根ID
        /// </summary>
        [DataMember(Name = "aggregateRootId")]
        public TAggregateRootId AggregateRootId { get; set; }

        protected override string GetRoutingKey()
        {
            if (AggregateRootId != null) return AggregateRootId.ToString();

            return null;
        }

        /// <summary>
        ///     输出字符串信息
        /// </summary>
        public override string ToString()
        {
            return string.Concat(GetType().FullName, "#", AggregateRootId);
        }
    }
}