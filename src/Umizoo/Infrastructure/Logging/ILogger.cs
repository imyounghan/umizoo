// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;

namespace Umizoo.Infrastructure.Logging
{
    /// <summary>
    /// 表示一个日志接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 是否启用Debug日志
        /// </summary>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// 是否启用Info日志
        /// </summary>
        bool IsInfoEnabled { get; }

        /// <summary>
        /// 写日志。
        /// </summary>
        void Debug(object message);
        /// <summary>
        /// 写日志。
        /// </summary>
        void Debug(object message, Exception exception);
        /// <summary>
        /// 写日志。
        /// </summary>
        void DebugFormat(string format, params object[] args);


        /// <summary>
        /// 写日志。
        /// </summary>
        void Info(object message);
        /// <summary>
        /// 写日志。
        /// </summary>
        void Info(object message, Exception exception);
        /// <summary>
        /// 写日志。
        /// </summary>
        void InfoFormat(string format, params object[] args);


        /// <summary>
        /// 写日志。
        /// </summary>
        void Warn(object message);
        /// <summary>
        /// 写日志。
        /// </summary>
        void Warn(object message, Exception exception);
        /// <summary>
        /// 写日志。
        /// </summary>
        void WarnFormat(string format, params object[] args);


        /// <summary>
        /// 写日志。
        /// </summary>
        void Error(object message);
        /// <summary>
        /// 写日志。
        /// </summary>
        void Error(object message, Exception exception);
        /// <summary>
        /// 写日志。
        /// </summary>
        void ErrorFormat(string format, params object[] args);

        /// <summary>
        /// 写日志。
        /// </summary>
        void Fatal(object message);
        /// <summary>
        /// 写日志。
        /// </summary>
        void Fatal(object message, Exception exception);
        /// <summary>
        /// 写日志。
        /// </summary>
        void FatalFormat(string format, params object[] args);
    }
}