

namespace Umizoo.Configurations
{
    using System;
    using System.IO;

    using log4net.Appender;
    using log4net.Config;
    using log4net.Layout;
    
    using Umizoo.Infrastructure;
    using Umizoo.Messaging;

    public static class ConfigurationExtentions
    {
        private static void ConfigureLog4Net()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            string binPath = string.IsNullOrEmpty(relativeSearchPath) ? baseDir : Path.Combine(baseDir, relativeSearchPath);
            string log4NetConfigFile = string.IsNullOrEmpty(binPath) ? "log4net.config" : Path.Combine(binPath, "log4net.config");

            if(File.Exists(log4NetConfigFile)) {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigFile));
            }
            else {
                BasicConfigurator.Configure(new ConsoleAppender { Layout = new PatternLayout() });
            }
        }

        public static Configuration UseKafka(this Configuration that, 
            ProcessingFlags flags = ProcessingFlags.All, 
            params string[] topics)
        {
            ConfigureLog4Net();

            topics.ForEach(KafkaUtils.CreateTopicIfNotExists);

            that.SetDefault<ITopicProvider, DefaultTopicProvider>();

            if(flags == ProcessingFlags.All || (flags & ProcessingFlags.Command) == ProcessingFlags.Command) {
                that.SetDefault<IMessageBus<ICommand>, CommandSender>();
                that.SetDefault<IMessageReceiver<Envelope<ICommand>>, CommandReceiver>();
            }
            if(flags == ProcessingFlags.All || (flags & ProcessingFlags.Event) == ProcessingFlags.Event) {
                that.SetDefault<IMessageBus<IEvent>, EventSender>();
                that.SetDefault<IMessageReceiver<Envelope<IEvent>>, EventReceiver>();
            }
            if(flags == ProcessingFlags.All || (flags & ProcessingFlags.PublishableException) == ProcessingFlags.PublishableException) {
                that.SetDefault<IMessageBus<IPublishableException>, PublishableExceptionSender>();
                that.SetDefault<IMessageReceiver<Envelope<IPublishableException>>, PublishableExceptionReceiver>();
            }
            if(flags == ProcessingFlags.All || (flags & ProcessingFlags.Query) == ProcessingFlags.Query) {
                that.SetDefault<IMessageBus<IQuery>, QuerySender>();
                that.SetDefault<IMessageReceiver<Envelope<IQuery>>, QueryReceiver>();
            }

            return that;
        }
    }
}
