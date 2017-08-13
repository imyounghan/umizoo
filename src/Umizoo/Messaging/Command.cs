// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Runtime.Serialization;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示继承该抽象类的是一个命令
    /// </summary>
    [DataContract]
    public abstract class Command : ICommand, IRoutingProvider
    {
        /// <summary>
        ///     默认构造函数
        /// </summary>
        protected Command()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        ///     生成当前消息的时间戳
        /// </summary>
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }


        #region IKeyProvider 成员

        string IRoutingProvider.GetRoutingKey()
        {
            return GetRoutingKey();
        }

        #endregion

        /// <summary>
        ///     获取路由的关键字
        /// </summary>
        protected virtual string GetRoutingKey()
        {
            return null;
        }
    }
}