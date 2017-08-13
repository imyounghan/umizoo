// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;

namespace Umizoo.Infrastructure.Logging
{
    /// <summary>
    /// 日志的扩展类
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// 写日志
        /// </summary>
        public static void Debug(this ILogger log, Exception ex)
        {
            log.Debug(ex.Message, ex);
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static void Debug(this ILogger log, Exception ex, string format, params object[] args)
        {
            log.Debug(string.Format(format, args), ex);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static void Info(this ILogger log, Exception ex)
        {
            log.Info(ex.Message, ex);
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static void Info(this ILogger log, Exception ex, string format, params object[] args)
        {
            log.Info(string.Format(format, args), ex);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static void Warn(this ILogger log, Exception ex)
        {
            log.Warn(ex.Message, ex);
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static void Warn(this ILogger log, Exception ex, string format, params object[] args)
        {
            log.Warn(string.Format(format, args), ex);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static void Error(this ILogger log, Exception ex)
        {
            log.Error(ex.Message, ex);
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static void Error(this ILogger log, Exception ex, string format, params object[] args)
        {
            log.Error(string.Format(format, args), ex);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static void Fatal(this ILogger log, Exception ex)
        {
            log.Fatal(ex.Message, ex);
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static void Fatal(this ILogger log, Exception ex, string format, params object[] args)
        {
            log.Fatal(string.Format(format, args), ex);
        }
    }
}