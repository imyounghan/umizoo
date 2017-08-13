// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections.Generic;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     表示存储事件的接口
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        ///     保存事件
        /// </summary>
        /// <param name="sourceInfo">数据源信息</param>
        /// <param name="events">事件集合</param>
        /// <param name="correlationId">相关的ID(如命令)</param>
        /// <returns>保存成功返回true，否则为false</returns>
        bool Save(SourceInfo sourceInfo, IEnumerable<IVersionedEvent> events, string correlationId);

        /// <summary>
        ///     查找事件。
        /// </summary>
        /// <param name="sourceInfo">数据源信息</param>
        /// <param name="correlationId">产生事件的相关Id</param>
        /// <returns>返回事件版本号和事件集合</returns>
        IEnumerable<IVersionedEvent> Find(SourceInfo sourceInfo, string correlationId);

        /// <summary>
        ///     查找事件。
        /// </summary>
        /// <param name="sourceInfo">数据源信息</param>
        /// <param name="startVersion">起始版本号</param>
        /// <returns>返回事件版本号和事件集合的集合</returns>
        IEnumerable<IVersionedEvent> FindAll(SourceInfo sourceInfo, int startVersion);
    }
}