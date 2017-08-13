// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

namespace Umizoo.Infrastructure.Filtering
{
    /// <summary>
    /// 表示继承该接口的是一个过滤器
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// 在类中实现时，获取是否允许多个过滤器的值。
        /// </summary>
        bool AllowMultiple { get; }

        /// <summary>
        /// 在类中实现时，获取过滤器顺序。
        /// </summary>
        int Order { get; }
    }
}