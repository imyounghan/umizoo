// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

namespace Umizoo.Infrastructure.Filtering
{
    /// <summary>
    /// 定义值，这些值指定过滤器在同一过滤器类型和过滤器顺序内的运行顺序。
    /// </summary>
    public enum FilterScope
    {
        /// <summary>
        /// 指定第一个
        /// </summary>
        First = 0,

        /// <summary>
        /// 指定在 <see cref="FilterScope.Class"/> 之前、<see cref="FilterScope.First"/> 之后的顺序。
        /// </summary>
        Global = 10,

        /// <summary>
        /// 指定在 <see cref="FilterScope.Method"/> 之前、<see cref="FilterScope.Global"/> 之后的顺序。
        /// </summary>
        Class = 20,

        /// <summary>
        /// 指定在 <see cref="FilterScope.Last"/> 之前、<see cref="FilterScope.Class"/> 之后的顺序。
        /// </summary>
        Method = 30,

        /// <summary>
        /// 指定最后一个。
        /// </summary>
        Last = 100,
    }
}