// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging.Handling
{
    public interface IQueryHandler : IHandler
    { }

    /// <summary>
    /// 继承该接口的是查询执行器
    /// </summary>
    public interface IQueryHandler<TQuery, TResult> : IQueryHandler
        where TQuery : IQuery
    {
        /// <summary>
        /// 获取结果
        /// </summary>
        TResult Handle(TQuery parameter);
    }    
}
