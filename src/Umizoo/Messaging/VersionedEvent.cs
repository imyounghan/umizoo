// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging
{
    public abstract class VersionedEvent : Event, IVersionedEvent
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; protected internal set; }
    }
}
