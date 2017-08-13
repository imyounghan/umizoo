// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Umizoo.Infrastructure.Logging
{
    public static class LogManager
    {
        private static readonly ConcurrentDictionary<string, ILogger> loggers;

        private static ILoggerFactory loggerFactory;

        static LogManager()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var binPath = string.IsNullOrEmpty(relativeSearchPath)
                ? baseDir
                : Path.Combine(baseDir, relativeSearchPath);
            var log4NetDllPath = string.IsNullOrEmpty(binPath) ? "log4net.dll" : Path.Combine(binPath, "log4net.dll");

            var log4netIsExist = File.Exists(log4NetDllPath)
                                 || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "log4net");

            if (log4netIsExist)
                loggerFactory = new Log4NetLoggerFactory();
            else
                loggerFactory = new NetFrameworkLoggerFactory();

            loggers = new ConcurrentDictionary<string, ILogger>();

            Default = GetLogger("Umizoo");
        }

        public static ILogger Default { get; }

        public static void SetLoggerFactory(Func<ILoggerFactory> loggerFactoryBuilder)
        {
            loggerFactory = loggerFactoryBuilder();
        }

        /// <summary>
        ///     获取一个写日志程序
        /// </summary>
        public static ILogger GetLogger(string name)
        {
            Assertions.NotNullOrWhiteSpace(name, "name");


            return loggers.GetOrAdd(name, loggerFactory.CreateLogger);
        }

        /// <summary>
        ///     获取一个写日志程序
        /// </summary>
        public static ILogger GetLogger(Type type)
        {
            Assertions.NotNull(type, "type");

            return loggers.GetOrAdd(type.FullName, loggerFactory.CreateLogger);
        }
    }
}