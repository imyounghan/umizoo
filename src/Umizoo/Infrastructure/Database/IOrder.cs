// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 排序表达式接口
    /// </summary>
    public interface IOrder<T> where T : class
    {
        /// <summary>
        /// 排序
        /// </summary>
        IEnumerable<IOrderItem<T>> OrderItems { get; }

        /// <summary>
        /// 排列计算
        /// </summary>
        IQueryable<T> Arranged(IQueryable<T> enumerable);
    }

    /// <summary>
    /// 排序元素接口
    /// </summary>
    public interface IOrderItem<T>
        where T : class
    {
        /// <summary>
        /// 获取排序lambda表达式
        /// </summary>
        Expression<Func<T, dynamic>> Expression { get; }

        /// <summary>
        /// 获取排序方式
        /// </summary>
        SortOrder SortOrder { get; }
    }
}
