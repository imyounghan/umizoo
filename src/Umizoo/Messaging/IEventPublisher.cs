// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections.Generic;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示继续该接口的是一个事件发布程序。
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        ///     获取待发布的事件。
        /// </summary>
        IEnumerable<IEvent> GetEvents();
    }
}