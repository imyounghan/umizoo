// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

namespace Umizoo.Infrastructure.Database
{
    public interface IDatabaseFactory
    {
        /// <summary>
        /// 当前上下文的数据访问
        /// </summary>
        IDatabase GetCurrent();

        IDatabase Create();

        IDatabase Create(string nameOrConnectionString);
    }
}
