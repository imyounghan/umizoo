// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

namespace Umizoo.Messaging
{
    /// <summary>
    /// 返回状态枚举定义
    /// </summary>
    public enum HandleStatus
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,
        /// <summary>
        /// 错误
        /// </summary>
        Failed = 1,
        /// <summary>
        /// 同步数据错误
        /// </summary>
        SyncFailed = 2,        
        /// <summary>
        /// 没有变化或数据
        /// </summary>
        Nothing = 3,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout = 4,
    }
}
