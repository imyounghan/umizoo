// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示继续该接口的是一个有标识的对象
    /// </summary>
    public interface IUniquelyIdentifiable
    {
        /// <summary>
        /// 标识ID
        /// </summary>
        string Id { get; }
    }
}
