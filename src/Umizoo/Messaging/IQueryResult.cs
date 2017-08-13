// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

namespace Umizoo.Messaging
{
    public interface IQueryResult : IResult
    {
        /// <summary>
        /// 结果数据
        /// </summary>
        object Data { get; }
    }
}
