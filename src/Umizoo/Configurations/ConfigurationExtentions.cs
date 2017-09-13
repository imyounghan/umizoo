// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure;
using Umizoo.Messaging;
using Umizoo.Messaging.Handling;
using Umizoo.Communication;

namespace Umizoo.Configurations
{
    /// <summary>
    ///     <see cref="Configuration" /> 的扩展类
    /// </summary>
    public static class ConfigurationExtentions
    {
        /// <summary>
        /// 加载程序集，如果为空就扫描目录
        /// </summary>
        public static Configuration LoadAssemblies(this Configuration that, params string[] assemblyNames)
        {
            Assembly[] assemblies = new Assembly[0];

            if (assemblyNames == null || assemblyNames.Length > 0) {
                assemblies = assemblyNames.Select(Assembly.Load).ToArray();
            }
            else {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
                string binPath = string.IsNullOrEmpty(relativeSearchPath)
                    ? baseDir
                    : Path.Combine(baseDir, relativeSearchPath);

                assemblies = Directory.GetFiles(binPath).Where(
                    file => {
                        string ext = Path.GetExtension(file).ToLower();
                        return ext.EndsWith(".dll") || ext.EndsWith(".exe");
                    }).Select(Assembly.LoadFrom).ToArray();
            }

            return that.LoadAssemblies(assemblies);
        }

        public static Configuration UseLocalQueue(this Configuration that, ProcessingFlags flags = Configuration.AllProcessingFlags)
        {
            that.Accept(container =>
            {
                if ((flags & ProcessingFlags.Command) == ProcessingFlags.Command) {
                    container.RegisterType<IMessageBus<ICommand>, MessageProducer<ICommand>>();
                    container.RegisterType<IMessageReceiver<Envelope<ICommand>>, MessageProducer<ICommand>>();
                }
                if ((flags & ProcessingFlags.Event) == ProcessingFlags.Event) {
                    container.RegisterType<IMessageBus<IEvent>, MessageProducer<IEvent>>();
                    container.RegisterType<IMessageReceiver<Envelope<IEvent>>, MessageProducer<IEvent>>();
                }
                if ((flags & ProcessingFlags.PublishableException) == ProcessingFlags.PublishableException) {
                    container.RegisterType<IMessageBus<IPublishableException>, MessageProducer<IPublishableException>>();
                    container.RegisterType<IMessageReceiver<Envelope<IPublishableException>>, MessageProducer<IPublishableException>>();
                }
                if ((flags & ProcessingFlags.Query) == ProcessingFlags.Query) {
                    container.RegisterType<IMessageBus<IQuery>, MessageProducer<IQuery>>();
                    container.RegisterType<IMessageReceiver<Envelope<IQuery>>, MessageProducer<IQuery>>();
                }
                if ((flags & ProcessingFlags.Result) == ProcessingFlags.Result) {
                    container.RegisterType<IMessageBus<IResult>, MessageProducer<IResult>>();
                    container.RegisterType<IMessageReceiver<Envelope<IResult>>, MessageProducer<IResult>>();
                }
            });


            return that.EnableProcessors(flags);
        }

        public static Configuration EnableProcessors(this Configuration that,
            ProcessingFlags processingFlags = Configuration.AllProcessingFlags)
        {
            that.Accept(container => {
                if ((processingFlags & ProcessingFlags.Command) == ProcessingFlags.Command) {
                    container.RegisterType<IProcessor, CommandConsumer>("command");
                }
                if ((processingFlags & ProcessingFlags.Event) == ProcessingFlags.Event) {
                    container.RegisterType<IProcessor, EventConsumer>("event");
                }
                if ((processingFlags & ProcessingFlags.PublishableException) == ProcessingFlags.PublishableException) {
                    container.RegisterType<IProcessor, PublishableExceptionConsumer>("exception");
                }
                if ((processingFlags & ProcessingFlags.Query) == ProcessingFlags.Query) {
                    container.RegisterType<IProcessor, QueryConsumer>("query");
                }
                if ((processingFlags & ProcessingFlags.Result) == ProcessingFlags.Result) {
                    container.RegisterType<IProcessor, ResultConsumer>("result");
                }
            });

            return that;
        }

        public static Configuration EnableService(this Configuration that, ConnectionMode connectionMode = ConnectionMode.Local)
        {
            that.Accept(container => {
                container.RegisterType<IEnvelopedMessageHandler<CommandResult>, ResultNotifyHandler>();
                container.RegisterType<IEnvelopedMessageHandler<QueryResult>, ResultNotifyHandler>();
            });
            

            switch (connectionMode) {
                case ConnectionMode.Local:
                    that.UseLocalQueue();
                    that.Accept(container => {
                        container.RegisterType<ICommandService, CommandService>();
                        container.RegisterType<IQueryService, QueryService>();
                    });
                    break;
                case ConnectionMode.Wcf:
                    that.Accept(container => {
                        container.RegisterType<IProcessor, WcfCommandServer>("commandService");
                        container.RegisterType<IProcessor, WcfQueryServer>("queryService");
                        container.RegisterType<IProcessor, WcfReplyServer>("replyService");
                    });
                    break;
                case ConnectionMode.Socket:
                    throw new NotImplementedException();
            }

            return that;
        }
    }
}