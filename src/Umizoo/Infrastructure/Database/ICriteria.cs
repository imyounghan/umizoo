// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 查询接口
    /// </summary>
    public interface ICriteria<T> where T : class
    {
        /// <summary>
        /// 获取lambda表达式
        /// </summary>
        Expression<Func<T, bool>> Expression { get; }

        /// <summary>
        /// 数据过滤
        /// </summary>
        IQueryable<T> Filtered(IQueryable<T> enumerable);
    }
}
