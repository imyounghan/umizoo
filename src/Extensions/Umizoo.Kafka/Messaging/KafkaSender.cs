

namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Kafka.Client.Messages;
    using Kafka.Client.Producers;

    using Umizoo.Infrastructure;

    public abstract class KafkaSender<TMessage, TDescriptor> : IMessageBus<TMessage>
        where TMessage : IMessage
        where TDescriptor : class, IDescriptor
    {
        private readonly static Lazy<Producer> _producer = new Lazy<Producer>(KafkaUtils.CreateProducer);

        private readonly ITextSerializer _serializer;

        private readonly string topic;

        public KafkaSender(ITextSerializer serializer, ITopicProvider topicProvider)
        {
            this._serializer = serializer;

            //this.messageKind = typeof(TMessage).FullName.Substring(1);
            this.topic = topicProvider.GetTopic(typeof(TDescriptor));
        }

        private void Push(params TDescriptor[] descriptors)
        {
            var producerDatas = new List<ProducerData<string, Message>>();
            foreach(var descriptor in descriptors) {
                var serialized = _serializer.SerializeToBytes(descriptor);
                var message = new Message(serialized);
                var key = descriptor.GetKey();

                producerDatas.Add(new ProducerData<string, Message>(topic, key, message));
            }
            

            var retryTimes = 0;
            var stopwatch = Stopwatch.StartNew();
            var maxtime = TimeSpan.FromMinutes(5);
            while (true) {
                try {
                    _producer.Value.Send(producerDatas);
                    break;
                }
                catch (Exception ex) {
                    if (stopwatch.Elapsed > maxtime) {
                        stopwatch.Stop();
                        LogManager.Default.Error(ex, "Sending messages to Kafka topic:'{0}' failed, it has been try {1} times.", topic, retryTimes);
                        break;
                    }
                }
                var waitTime = Math.Min((int)Math.Pow(2, ++retryTimes), 60);
                Thread.Sleep(waitTime * 500);
            }

            if(LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat("Send messages to Kafka topic:'{0}' successfully.", topic);
            }
        }


        protected abstract string GetLogInfo(TDescriptor descriptor);

        protected abstract TDescriptor Convert(Envelope<TMessage> envelope, ITextSerializer serializer);

        #region IMessageBus<TMessage> 成员

        public void Send(Envelope<TMessage> message)
        {
            var descriptor = this.Convert(message, _serializer);

            //if(LogManager.Default.IsDebugEnabled) {
            //    LogManager.Default.DebugFormat("Ready to send a {0}:{1} to kafka.", messageKind, this.GetLogInfo(descriptor));
            //}

            this.Push(descriptor);
        }

        public void Send(IEnumerable<Envelope<TMessage>> messages)
        {
            var descriptors = messages.Select(message => this.Convert(message, _serializer)).ToArray();

            //if(LogManager.Default.IsDebugEnabled) {
            //    LogManager.Default.DebugFormat("Ready to send a batch of {0}s:[[{1}]] to kafka.", 
            //        messageKind,
            //        string.Join("],[", descriptors.Select(this.GetLogInfo)));
            //}

            this.Push(descriptors);
        }

        #endregion
    }

}
