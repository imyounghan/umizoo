// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


namespace Umizoo.Messaging
{
    /// <summary>A enum defines the command result return type.
    /// </summary>
    public enum CommandReturnMode
    {
        /// <summary>
        /// 表示命令发送成功就返回
        /// </summary>
        Delivered = 0,
        /// <summary>
        /// 表示命令执行完成
        /// </summary>
        CommandExecuted = 1,
        /// <summary>
        /// 表示由命令引发的事件处理完成
        /// </summary>
        EventHandled = 2,
        /// <summary>
        /// 表示需要手动回复
        /// </summary>
        Manual
    }
}
