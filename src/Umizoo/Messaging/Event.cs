// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using System;
using System.Runtime.Serialization;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示继承该抽象类的是一个事件
    /// </summary>
    [DataContract]
    public abstract class Event : IEvent
    {
        /// <summary>
        ///     默认构造函数
        /// </summary>
        protected Event()
        {
            CreationTime = DateTime.UtcNow;
        }

        /// <summary>
        ///     创建时间
        /// </summary>
        [DataMember(Name = "creationTime")]
        public DateTime CreationTime { get; set; }
    }
}