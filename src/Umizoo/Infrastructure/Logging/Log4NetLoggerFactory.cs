// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Umizoo.Infrastructure.Logging
{
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private readonly Func<Type, object> _getLoggerByTypeDelegate;

        public Log4NetLoggerFactory()
        {
            var log4netManagerType = Type.GetType("log4net.LogManager, log4net");
            _getLoggerByNameDelegate = GetGetLoggerMethodCall<string>(log4netManagerType);
            _getLoggerByTypeDelegate = GetGetLoggerMethodCall<Type>(log4netManagerType);
        }


        private static Func<TParameter, object> GetGetLoggerMethodCall<TParameter>(Type log4netManagerType)
        {
            MethodInfo method = log4netManagerType.GetMethod("GetLogger", new[] { typeof(TParameter) });
            ParameterExpression resultValue;
            ParameterExpression keyParam = Expression.Parameter(typeof(TParameter), "key");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] { resultValue = keyParam });
            return Expression.Lambda<Func<TParameter, object>>(methodCall, new[] { resultValue }).Compile();
        }

        public ILogger CreateLogger(string name)
        {
            return new Log4NetLogger(_getLoggerByNameDelegate(name));
        }

        public ILogger CreateLogger(Type type)
        {
            return new Log4NetLogger(_getLoggerByTypeDelegate(type));
        }
    }
}