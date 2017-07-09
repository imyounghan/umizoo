namespace Umizoo.Infrastructure
{
    using System;

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
        /// 是否启用Warn日志
        /// </summary>
        bool IsWarnEnabled { get; }

        /// <summary>
        /// 是否启用Error日志
        /// </summary>
        bool IsErrorEnabled { get; }

        /// <summary>
        /// 是否启用Fatal日志
        /// </summary>
        bool IsFatalEnabled { get; }

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

        //ILoggerFactory Factory { get; }
    }


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
