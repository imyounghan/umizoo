// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;

namespace Umizoo.Seeds
{
    /// <summary>
    /// 表示聚合根的持久化接口
    /// </summary>
    public interface IAggregateStorage
    {
        /// <summary>
        /// 查找聚合。如果不存在返回null，存在返回实例
        /// </summary>
        IAggregateRoot Find(Type aggregateRootType, object aggregateRootId);

        void Save(IAggregateRoot aggregateRoot);

        void Delete(IAggregateRoot aggregateRoot);
    }
}