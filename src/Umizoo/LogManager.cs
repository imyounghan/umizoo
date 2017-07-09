
namespace Umizoo
{
    using System;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Umizoo.Infrastructure;


    public static class LogManager
    {
        private static readonly Func<string, object> GetLoggerByNameDelegate;
        private static readonly Func<Type, object> GetLoggerByTypeDelegate;

        private static readonly Type Log4netManagerType;
        private static readonly bool log4netIsExist;

        private static readonly ConcurrentDictionary<string, ILogger> loggers;

        static LogManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            string binPath = string.IsNullOrEmpty(relativeSearchPath)
                                 ? baseDir
                                 : Path.Combine(baseDir, relativeSearchPath);
            string log4NetDllPath = string.IsNullOrEmpty(binPath) ? "log4net.dll" : Path.Combine(binPath, "log4net.dll");

            log4netIsExist = File.Exists(log4NetDllPath)
                             || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "log4net");

            if (log4netIsExist) {
                Log4netManagerType = Type.GetType("log4net.LogManager, log4net");
                GetLoggerByNameDelegate = GetGetLoggerMethodCall<string>();
                GetLoggerByTypeDelegate = GetGetLoggerMethodCall<Type>();
            }

            loggers = new ConcurrentDictionary<string, ILogger>();

            Default = GetLogger("Umizoo");
        }

        public static ILogger Default { get; private set; }

        //public static ILogger GetDefaultLogger()
        //{
        //    return GetLogger("Umizoo");
        //}

        /// <summary>
        /// 获取一个写日志程序
        /// </summary>
        public static ILogger GetLogger(string name)
        {
            Ensure.NotNullOrWhiteSpace(name, "name");

            if (log4netIsExist) {
                return loggers.GetOrAdd(name, () => new Log4NetLogger(GetLoggerByNameDelegate(name)));
            }

            return loggers.GetOrAdd(name, NetFrameworkLogger.Create);
        }

        /// <summary>
        /// 获取一个写日志程序
        /// </summary>
        public static ILogger GetLogger(Type type)
        {
            Ensure.NotNull(type, "type");

            if (log4netIsExist) {
                return loggers.GetOrAdd(type.FullName, () => new Log4NetLogger(GetLoggerByTypeDelegate(type)));
            }

            return loggers.GetOrAdd(type.FullName, NetFrameworkLogger.Create);
        }


        private static Func<TParameter, object> GetGetLoggerMethodCall<TParameter>()
        {
            MethodInfo method = Log4netManagerType.GetMethod("GetLogger", new[] { typeof(TParameter) });
            ParameterExpression resultValue;
            ParameterExpression keyParam = Expression.Parameter(typeof(TParameter), "key");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] { resultValue = keyParam });
            return Expression.Lambda<Func<TParameter, object>>(methodCall, new[] { resultValue }).Compile();
        }

        private class NetFrameworkLogger : ILogger
        {
            public static ILogger Create(string category)
            {
                return new NetFrameworkLogger(category);
            }

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

            private NetFrameworkLogger(string category)
            {
                this.category = category;
            }

            public bool IsDebugEnabled
            {
                get { return IsContain(LogPriority, PriorityFlags.DEBUG); }
            }

            public bool IsErrorEnabled
            {
                get { return IsContain(LogPriority, PriorityFlags.ERROR); }
            }

            public bool IsFatalEnabled
            {
                get { return IsContain(LogPriority, PriorityFlags.FATAL); }
            }

            public bool IsInfoEnabled
            {
                get { return IsContain(LogPriority, PriorityFlags.INFO); }
            }

            public bool IsWarnEnabled
            {
                get { return IsContain(LogPriority, PriorityFlags.WARN); }
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
                if (!this.IsErrorEnabled) {
                    return;
                }

                WriteLog(category, PriorityFlags.ERROR, message.ToString(), exception);
            }

            public void ErrorFormat(string format, params object[] args)
            {
                if (!this.IsErrorEnabled) {
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
                if (!this.IsFatalEnabled) {
                    return;
                }

                WriteLog(category, PriorityFlags.FATAL, message.ToString(), exception);
            }

            public void FatalFormat(string format, params object[] args)
            {
                if (!this.IsFatalEnabled) {
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
                if (!this.IsWarnEnabled) {
                    return;
                }

                WriteLog(category, PriorityFlags.WARN, message.ToString(), exception);
            }

            public void WarnFormat(string format, params object[] args)
            {
                if (!this.IsWarnEnabled) {
                    return;
                }

                WriteLog(category, PriorityFlags.WARN, string.Format(format, args), null);
            }
        }

        private class Log4NetLogger : ILogger
        {
            #region Fields

            private static readonly Type ILogType;

            private static readonly Func<object, bool> IsDebugEnabledDelegate;

            private static readonly Func<object, bool> IsErrorEnabledDelegate;

            private static readonly Func<object, bool> IsFatalEnabledDelegate;

            private static readonly Func<object, bool> IsInfoEnabledDelegate;

            private static readonly Func<object, bool> IsWarnEnabledDelegate;

            private static readonly Action<object, object> DebugDelegate;

            private static readonly Action<object, object, Exception> DebugExceptionDelegate;

            private static readonly Action<object, string, object[]> DebugFormatDelegate;

            private static readonly Action<object, object> ErrorDelegate;

            private static readonly Action<object, object, Exception> ErrorExceptionDelegate;

            private static readonly Action<object, string, object[]> ErrorFormatDelegate;

            private static readonly Action<object, object> FatalDelegate;

            private static readonly Action<object, object, Exception> FatalExceptionDelegate;

            private static readonly Action<object, string, object[]> FatalFormatDelegate;

            private static readonly Action<object, object> InfoDelegate;

            private static readonly Action<object, object, Exception> InfoExceptionDelegate;

            private static readonly Action<object, string, object[]> InfoFormatDelegate;

            private static readonly Action<object, object> WarnDelegate;

            private static readonly Action<object, object, Exception> WarnExceptionDelegate;

            private static readonly Action<object, string, object[]> WarnFormatDelegate;


            private readonly object logger;

            #endregion

            #region Constructors and Destructors

            static Log4NetLogger()
            {
                ILogType = Type.GetType("log4net.ILog, log4net");

                IsErrorEnabledDelegate = GetPropertyGetter("IsErrorEnabled");
                IsFatalEnabledDelegate = GetPropertyGetter("IsFatalEnabled");
                IsDebugEnabledDelegate = GetPropertyGetter("IsDebugEnabled");
                IsInfoEnabledDelegate = GetPropertyGetter("IsInfoEnabled");
                IsWarnEnabledDelegate = GetPropertyGetter("IsWarnEnabled");
                ErrorDelegate = GetMethodCallForMessage("Error");
                ErrorExceptionDelegate = GetMethodCallForMessageException("Error");
                ErrorFormatDelegate = GetMethodCallForMessageFormat("ErrorFormat");

                FatalDelegate = GetMethodCallForMessage("Fatal");
                FatalExceptionDelegate = GetMethodCallForMessageException("Fatal");
                FatalFormatDelegate = GetMethodCallForMessageFormat("FatalFormat");

                DebugDelegate = GetMethodCallForMessage("Debug");
                DebugExceptionDelegate = GetMethodCallForMessageException("Debug");
                DebugFormatDelegate = GetMethodCallForMessageFormat("DebugFormat");

                InfoDelegate = GetMethodCallForMessage("Info");
                InfoExceptionDelegate = GetMethodCallForMessageException("Info");
                InfoFormatDelegate = GetMethodCallForMessageFormat("InfoFormat");

                WarnDelegate = GetMethodCallForMessage("Warn");
                WarnExceptionDelegate = GetMethodCallForMessageException("Warn");
                WarnFormatDelegate = GetMethodCallForMessageFormat("WarnFormat");
            }

            public Log4NetLogger(object logger)
            {
                this.logger = logger;
            }

            #endregion

            #region Public Properties

            public bool IsDebugEnabled
            {
                get { return IsDebugEnabledDelegate(this.logger); }
            }

            public bool IsErrorEnabled
            {
                get { return IsErrorEnabledDelegate(this.logger); }
            }

            public bool IsFatalEnabled
            {
                get { return IsFatalEnabledDelegate(this.logger); }
            }

            public bool IsInfoEnabled
            {
                get { return IsInfoEnabledDelegate(this.logger); }
            }

            public bool IsWarnEnabled
            {
                get { return IsWarnEnabledDelegate(this.logger); }
            }

            #endregion

            #region Methods and Operators

            public void Debug(object message)
            {
                this.Debug(message, null);
            }

            public void Debug(object message, Exception exception)
            {
                if (!this.IsDebugEnabled) {
                    return;
                }

                if (exception == null) {
                    DebugDelegate(this.logger, message);
                }
                else {
                    DebugExceptionDelegate(this.logger, message, exception);
                }
            }

            public void DebugFormat(string format, params object[] args)
            {
                if (this.IsDebugEnabled) {
                    DebugFormatDelegate(this.logger, format, args);
                }
            }

            public void Error(object message)
            {
                this.Error(message, null);
            }

            public void Error(object message, Exception exception)
            {
                if (!this.IsErrorEnabled) {
                    return;
                }

                if (exception == null) {
                    ErrorDelegate(this.logger, message);
                }
                else {
                    ErrorExceptionDelegate(this.logger, message, exception);
                }
            }

            public void ErrorFormat(string format, params object[] args)
            {
                if (this.IsErrorEnabled) {
                    ErrorFormatDelegate(this.logger, format, args);
                }
            }

            public void Fatal(object message)
            {
                this.Fatal(message, null);
            }

            public void Fatal(object message, Exception exception)
            {
                if (!this.IsFatalEnabled) {
                    return;
                }

                if (exception == null) {
                    FatalDelegate(this.logger, message);
                }
                else {
                    FatalExceptionDelegate(this.logger, message, exception);
                }
            }

            public void FatalFormat(string format, params object[] args)
            {
                if (this.IsFatalEnabled) {
                    FatalFormatDelegate(this.logger, format, args);
                }
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

                if (exception == null) {
                    InfoDelegate(this.logger, message);
                }
                else {
                    InfoExceptionDelegate(this.logger, message, exception);
                }
            }

            public void InfoFormat(string format, params object[] args)
            {
                if (this.IsInfoEnabled) {
                    InfoFormatDelegate(this.logger, format, args);
                }
            }

            public void Warn(object message)
            {
                this.Warn(message, null);
            }

            public void Warn(object message, Exception exception)
            {
                if (!this.IsWarnEnabled) {
                    return;
                }

                if (exception == null) {
                    WarnDelegate(this.logger, message);
                }
                else {
                    WarnExceptionDelegate(this.logger, message, exception);
                }
            }

            public void WarnFormat(string format, params object[] args)
            {
                if (this.IsWarnEnabled) {
                    WarnFormatDelegate(this.logger, format, args);
                }
            }


            private static Action<object, object> GetMethodCallForMessage(string methodName)
            {
                ParameterExpression loggerParam = Expression.Parameter(typeof(object), "l");
                ParameterExpression messageParam = Expression.Parameter(typeof(object), "o");
                Expression convertedParam = Expression.Convert(loggerParam, ILogType);
                MethodCallExpression methodCall = Expression.Call(
                    convertedParam,
                    ILogType.GetMethod(methodName, new[] { typeof(object) }),
                    messageParam);
                return
                    (Action<object, object>)Expression.Lambda(methodCall, new[] { loggerParam, messageParam }).Compile();
            }

            private static Action<object, object, Exception> GetMethodCallForMessageException(string methodName)
            {
                ParameterExpression loggerParam = Expression.Parameter(typeof(object), "l");
                ParameterExpression messageParam = Expression.Parameter(typeof(object), "o");
                ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception), "e");
                Expression convertedParam = Expression.Convert(loggerParam, ILogType);
                MethodCallExpression methodCall = Expression.Call(
                    convertedParam,
                    ILogType.GetMethod(methodName, new[] { typeof(object), typeof(Exception) }),
                    messageParam,
                    exceptionParam);
                return
                    (Action<object, object, Exception>)
                    Expression.Lambda(methodCall, new[] { loggerParam, messageParam, exceptionParam }).Compile();
            }

            private static Action<object, string, object[]> GetMethodCallForMessageFormat(string methodName)
            {
                ParameterExpression loggerParam = Expression.Parameter(typeof(object), "l");
                ParameterExpression formatParam = Expression.Parameter(typeof(string), "f");
                ParameterExpression parametersParam = Expression.Parameter(typeof(object[]), "p");
                Expression convertedParam = Expression.Convert(loggerParam, ILogType);
                MethodCallExpression methodCall = Expression.Call(
                    convertedParam,
                    ILogType.GetMethod(methodName, new[] { typeof(string), typeof(object[]) }),
                    formatParam,
                    parametersParam);
                return
                    (Action<object, string, object[]>)
                    Expression.Lambda(methodCall, new[] { loggerParam, formatParam, parametersParam }).Compile();
            }

            private static Func<object, bool> GetPropertyGetter(string propertyName)
            {
                ParameterExpression funcParam = Expression.Parameter(typeof(object), "l");
                Expression convertedParam = Expression.Convert(funcParam, ILogType);
                Expression property = Expression.Property(convertedParam, propertyName);
                return (Func<object, bool>)Expression.Lambda(property, funcParam).Compile();
            }

            #endregion
        }
    }
}
