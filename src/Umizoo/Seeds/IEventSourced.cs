// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections.Generic;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    /// <summary>
    ///     表示继承该接口的是一个通过事件溯源当前状态的聚合根
    /// </summary>
    public interface IEventSourced : IAggregateRoot
    {
        /// <summary>
        ///     表示当前的版本号
        /// </summary>
        int Version { get; }

        ///// <summary>
        ///// 接受变更版本号
        ///// </summary>
        //void AcceptChanges(int newVersion);
        IEnumerable<IVersionedEvent> GetChanges();

        /// <summary>
        ///     加载事件。
        /// </summary>
        void LoadFrom(IEnumerable<IVersionedEvent> events);
    }
}