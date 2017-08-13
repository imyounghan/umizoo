// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using System;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     表示继承该接口的是一个溯源事件的上下文
    /// </summary>
    public interface IEventContext
    {
        /// <summary>
        ///     产生事件的聚合信息
        /// </summary>
        SourceInfo SourceInfo { get; }

        /// <summary>
        ///     产生事件的命令信息
        /// </summary>
        SourceInfo CommandInfo { get; }

        /// <summary>
        ///     添加一个命令到当前上下文
        /// </summary>
        void AddCommand(ICommand command);

        void CompleteCommand(object result = null, Func<object, string> serializer = null);
    }
}