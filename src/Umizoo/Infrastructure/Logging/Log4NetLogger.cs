// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Linq.Expressions;

namespace Umizoo.Infrastructure.Logging
{
    public class Log4NetLogger : ILogger
    {
        private static readonly Type ILogType;

        private static readonly Func<object, bool> IsDebugEnabledDelegate;
        private static readonly Func<object, bool> IsInfoEnabledDelegate;

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


        static Log4NetLogger()
        {
            ILogType = Type.GetType("log4net.ILog, log4net");

            IsDebugEnabledDelegate = GetPropertyGetter("IsDebugEnabled");
            IsInfoEnabledDelegate = GetPropertyGetter("IsInfoEnabled");

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

        public bool IsDebugEnabled
        {
            get { return IsDebugEnabledDelegate(this.logger); }
        }

        public bool IsInfoEnabled
        {
            get { return IsInfoEnabledDelegate(this.logger); }
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
            if (exception == null) {
                ErrorDelegate(this.logger, message);
            }
            else {
                ErrorExceptionDelegate(this.logger, message, exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            ErrorFormatDelegate(this.logger, format, args);
        }

        public void Fatal(object message)
        {
            this.Fatal(message, null);
        }

        public void Fatal(object message, Exception exception)
        {
            if (exception == null) {
                FatalDelegate(this.logger, message);
            }
            else {
                FatalExceptionDelegate(this.logger, message, exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            FatalFormatDelegate(this.logger, format, args);
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
            if (exception == null) {
                WarnDelegate(this.logger, message);
            }
            else {
                WarnExceptionDelegate(this.logger, message, exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            WarnFormatDelegate(this.logger, format, args);
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
    }
}