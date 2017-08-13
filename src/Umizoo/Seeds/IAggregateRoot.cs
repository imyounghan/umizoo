// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

using Umizoo.Infrastructure;

namespace Umizoo.Seeds
{
    /// <summary>
    ///     表示继承该接口的类型是一个聚合根
    /// </summary>
    public interface IAggregateRoot : IUniquelyIdentifiable
    {
    }
}