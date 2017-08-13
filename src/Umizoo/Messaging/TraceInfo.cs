// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

namespace Umizoo.Messaging
{
    /// <summary>
    ///     描述跟踪信息
    /// </summary>
    public class TraceInfo
    {
        public TraceInfo(string traceId, string traceAddress)
        {
            Id = traceId;
            Address = traceAddress;
        }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     跟踪ID
        /// </summary>
        public string Id { get; }

        public override bool Equals(object obj)
        {
            var other = obj as TraceInfo;
            if (other == null) return false;

            return Id == other.Id && Address == other.Address;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}