// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示这是一个任务
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// 启动任务
        /// </summary>
        void Start();

        /// <summary>
        /// 停止任务
        /// </summary>
        void Stop();
    }
}
