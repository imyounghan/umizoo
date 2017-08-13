// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Threading.Tasks;

namespace Umizoo.Messaging
{
    /// <summary>
    /// 表示这是一个查询服务
    /// </summary>
    public interface IQueryService
    {
        //Task<IQueryResult> FetchAsync(IQuery query, int timeoutMs = 120000);

        /// <summary>
        /// 读取数据
        /// </summary>
        T Fetch<T>(IQuery query, int timeoutMs = 120000);

        Task<T> FetchAsync<T>(IQuery query, int timeoutMs = 120000);

        int WaitingRequests { get; }
    }
}
