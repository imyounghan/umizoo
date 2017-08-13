using System;
using System.IO;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Composition;
using Umizoo.Messaging;

namespace Umizoo.Configurations
{
    public static class ConfigurationExtentions
    {
        private static void ConfigureLog4Net()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var binPath = string.IsNullOrEmpty(relativeSearchPath)
                ? baseDir
                : Path.Combine(baseDir, relativeSearchPath);
            var log4NetConfigFile = string.IsNullOrEmpty(binPath)
                ? "log4net.config"
                : Path.Combine(binPath, "log4net.config");

            if (File.Exists(log4NetConfigFile)) XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigFile));
            else BasicConfigurator.Configure(new ConsoleAppender {Layout = new PatternLayout()});
        }

        public static Configuration UseKafka(this Configuration that,
            ProcessingFlags flags = Configuration.AllProcessingFlags,
            params string[] topics)
        {
            ConfigureLog4Net();

            topics.ForEach(KafkaUtils.CreateTopicIfNotExists);


            return that.Accept(container =>
            {
                container.RegisterType<ITopicProvider, DefaultTopicProvider>();

                if ((flags & ProcessingFlags.Command) == ProcessingFlags.Command)
                {
                    container.RegisterType<IMessageBus<ICommand>, CommandSender>();
                    container.RegisterType<IMessageReceiver<Envelope<ICommand>>, CommandReceiver>();
                }
                if ((flags & ProcessingFlags.Event) == ProcessingFlags.Event)
                {
                    container.RegisterType<IMessageBus<IEvent>, EventSender>();
                    container.RegisterType<IMessageReceiver<Envelope<IEvent>>, EventReceiver>();
                }
                if ((flags & ProcessingFlags.PublishableException) == ProcessingFlags.PublishableException)
                {
                    container.RegisterType<IMessageBus<IPublishableException>, PublishableExceptionSender>();
                    container
                        .RegisterType<IMessageReceiver<Envelope<IPublishableException>>, PublishableExceptionReceiver
                        >();
                }
                if ((flags & ProcessingFlags.Query) == ProcessingFlags.Query)
                {
                    container.RegisterType<IMessageBus<IQuery>, QuerySender>();
                    container.RegisterType<IMessageReceiver<Envelope<IQuery>>, QueryReceiver>();
                }
            });
        }
    }
}