// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Runtime.Serialization;

namespace Umizoo.Messaging
{
    /// <summary>
    ///     分页查询参数的抽象类
    /// </summary>
    [DataContract]
    public abstract class PageQuery : IQuery
    {
        /// <summary>
        ///     当前页码
        /// </summary>
        [DataMember(Name = "pageIndex")]
        public int PageIndex { get; set; }

        /// <summary>
        ///     当前页显示数量大小
        /// </summary>
        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }
    }
}