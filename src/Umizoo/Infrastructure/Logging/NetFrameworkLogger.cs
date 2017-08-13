// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Umizoo.Infrastructure.Logging
{
    public class NetFrameworkLogger : ILogger
    {
        [Flags]
        private enum PriorityFlags
        {
            DEBUG = 1,
            INFO = 2,
            WARN = 4,
            ERROR = 8,
            FATAL = 16
        }

        private static readonly string LogAppender;
        private static readonly PriorityFlags LogPriority;

        static NetFrameworkLogger()
        {
            LogAppender = ConfigurationManager.AppSettings["umizoo.log_appender"].IfEmpty("FILE").ToLower();
            LogPriority = GetLogPriority(ConfigurationManager.AppSettings["umizoo.log_priority"].IfEmpty("OFF"));

            if (LogAppender == "all" || LogAppender == "console") {
                Trace.Listeners.Add(new ConsoleTraceListener(false));
            }

            if (LogAppender == "all" || LogAppender == "file") {
                string logFile = CreateFile();
                Trace.Listeners.Add(new TextWriterTraceListener(logFile));
                Trace.AutoFlush = true;
            }
        }

        private static PriorityFlags GetLogPriority(string priority)
        {
            switch (priority.ToLower()) {
                case "debug":
                    priority += "|info|warn|error|fatal";
                    break;
                case "info":
                    priority += "|warn|error|fatal";
                    break;
                case "warn":
                    priority += "|error|fatal";
                    break;
                case "error":
                    priority += "|fatal";
                    break;
                case "fatal":
                    break;
                default:
                    return (PriorityFlags)(-1);
            }

            var logPriority = default(PriorityFlags);

            priority.Split('|').ForEach(item => {
                PriorityFlags temp;
                if (!Enum.TryParse(item, true, out temp)) {
                    return;
                }

                logPriority |= temp;
            });

            return logPriority;
        }

        private static bool IsContain(PriorityFlags priority, PriorityFlags comparer)
        {
            return (priority & comparer) == comparer;
        }

        private static string GetMapPath(string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            string binPath = string.IsNullOrEmpty(relativeSearchPath)
                ? baseDir
                : Path.Combine(baseDir, relativeSearchPath);
            return Path.Combine(binPath, fileName);
        }

        private static string CreateFile()
        {
            string today = DateTime.Today.ToString("yyyyMMdd");
            string filename = GetMapPath(string.Concat("log\\log_", today, ".txt"));
            int fileIndex = 0;

            while (true) {
                if (!File.Exists(filename)) {
                    return filename;
                }

                filename = GetMapPath(string.Concat("log\\log_", today, "_", ++fileIndex, ".txt"));
            }
        }

        private static void WriteLog(string category, PriorityFlags logpriority, string message, Exception exception)
        {
            if (!IsContain(LogPriority, logpriority)) {
                return;
            }

            StringBuilder log = new StringBuilder()
                .Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .AppendFormat(" {0} [{1}]", logpriority,
                    Thread.CurrentThread.Name.IfEmpty(() => Thread.CurrentThread.ManagedThreadId.ToString().PadRight(5)));
            if (!string.IsNullOrWhiteSpace(message)) {
                log.Append(" Message:").Append(message);
            }

            if (exception != null) {
                log.Append(" Exception:").Append(exception);
                if (exception.InnerException != null) {
                    log.AppendLine().Append("InnerException:").Append(exception.InnerException);
                }
            }

            Trace.WriteLine(log.ToString(), category);
        }



        private readonly string category;

        public NetFrameworkLogger(string category)
        {
            this.category = category;
        }

        public bool IsDebugEnabled
        {
            get { return IsContain(LogPriority, PriorityFlags.DEBUG); }
        }

        public bool IsInfoEnabled
        {
            get { return IsContain(LogPriority, PriorityFlags.INFO); }
        }

        public void Debug(object message)
        {
            this.Debug(message, null);
        }

        public void Debug(object message, Exception exception)
        {
            if (!this.IsDebugEnabled) {
                return;
            }

            WriteLog(category, PriorityFlags.DEBUG, message.ToString(), exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (!this.IsDebugEnabled) {
                return;
            }

            WriteLog(category, PriorityFlags.DEBUG, string.Format(format, args), null);
        }

        public void Error(object message)
        {
            this.Error(message, null);
        }

        public void Error(object message, Exception exception)
        {
            if (!IsContain(LogPriority, PriorityFlags.ERROR)) {
                return;
            }

            WriteLog(category, PriorityFlags.ERROR, message.ToString(), exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (!IsContain(LogPriority, PriorityFlags.ERROR)) {
                return;
            }

            WriteLog(category, PriorityFlags.ERROR, string.Format(format, args), null);
        }

        public void Fatal(object message)
        {
            this.Fatal(message, null);
        }

        public void Fatal(object message, Exception exception)
        {
            if (!IsContain(LogPriority, PriorityFlags.FATAL)) {
                return;
            }

            WriteLog(category, PriorityFlags.FATAL, message.ToString(), exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (!IsContain(LogPriority, PriorityFlags.FATAL)) {
                return;
            }

            WriteLog(category, PriorityFlags.FATAL, string.Format(format, args), null);
        }

        public void Info(object message)
        {
            this.Info(message, null);
        }

        public void Info(object message, Exception exception)
        {
            if (!this.IsInfoEnabled) {
                return;
            }

            WriteLog(category, PriorityFlags.INFO, message.ToString(), exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (!this.IsInfoEnabled) {
                return;
            }

            WriteLog(category, PriorityFlags.INFO, string.Format(format, args), null);
        }

        public void Warn(object message)
        {
            this.Warn(message, null);
        }

        public void Warn(object message, Exception exception)
        {
            if (!IsContain(LogPriority, PriorityFlags.WARN)) {
                return;
            }

            WriteLog(category, PriorityFlags.WARN, message.ToString(), exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (!IsContain(LogPriority, PriorityFlags.WARN)) {
                return;
            }

            WriteLog(category, PriorityFlags.WARN, string.Format(format, args), null);
        }
    }
}