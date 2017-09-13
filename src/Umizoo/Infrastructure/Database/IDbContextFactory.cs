// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 创建数据上下文的工厂
    /// </summary>
    public interface IDbContextFactory
    {
        /// <summary>
        /// 当前上下文的数据操作
        /// </summary>
        IDbContext GetCurrent();

        /// <summary>
        /// 打开一个数据操作
        /// </summary>
        IDbContext Create();

        /// <summary>
        /// 打开一个数据操作
        /// </summary>
        IDbContext Create(string nameOrConnectionString);
    }
}
