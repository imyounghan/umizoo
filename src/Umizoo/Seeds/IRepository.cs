// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    /// <summary>
    /// 表示继承该接口的是一个聚合根仓储。
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// 查找聚合。如果不存在返回null，存在返回实例
        /// </summary>
        IAggregateRoot Find(Type aggregateRootType, object aggregateRootId);

        /// <summary>
        /// 保存聚合根。
        /// </summary>
        void Save(IAggregateRoot aggregateRoot);

        /// <summary>
        /// 保存聚合根。
        /// </summary>
        void Save(IEventSourced eventSourced, Envelope<ICommand> command);

        ///// <summary>
        ///// 删除聚合根。
        ///// </summary>
        //void Delete(IAggregateRoot aggregateRoot);
    }

    /// <summary>
    /// 表示继承该接口的是一个仓储。
    /// </summary>
    public interface IRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// 添加聚合到仓储
        /// </summary>
        void Add(TAggregateRoot aggregateRoot);
        /// <summary>
        /// 更新聚合到仓储
        /// </summary>
        void Update(TAggregateRoot aggregateRoot);
        /// <summary>
        /// 从仓储中移除聚合
        /// </summary>
        void Remove(TAggregateRoot aggregateRoot);

        /// <summary>
        /// 查找聚合。如果不存在返回null，存在返回实例
        /// </summary>
        TAggregateRoot Find<TIdentify>(TIdentify id);
    }
}
