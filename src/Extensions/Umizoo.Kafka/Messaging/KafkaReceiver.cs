using System;
using System.Collections.Generic;
using System.Threading;
using Kafka.Client.Consumers;
using Kafka.Client.Messages;
using Kafka.Client.Serialization;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging
{
    public abstract class KafkaReceiver<TDescriptor, TMessage> : MessageReceiver<TMessage>
        where TDescriptor : class, IDescriptor
        where TMessage : IMessage
    {
        private readonly IDecoder<Message> _decoder;

        private readonly ITextSerializer _serializer;
        private readonly string _topic;
        private ZookeeperConsumerConnector _consumer;

        protected KafkaReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
        {
            _serializer = serializer;
            _decoder = new DefaultDecoder();
            _topic = topicProvider.GetTopic(typeof(TDescriptor));
        }
        
        protected override void Start()
        {
            _consumer = KafkaUtils.CreateBalancedConsumer(_topic);
        }

        protected override void Stop()
        {
            _consumer.Dispose();
            _consumer = null;
        }

        protected abstract Envelope<TMessage> Convert(TDescriptor descriptor, ITextSerializer serializer);

        private void ProcessingMessage(Message message)
        {
            if (LogManager.Default.IsDebugEnabled)
                LogManager.Default.DebugFormat(
                    "Pull a message from kafka on topic('{0}'). offset:{1}, partition:{2}.",
                    _topic,
                    message.Offset,
                    message.PartitionId);

            try
            {
                var serialized = _serializer.Deserialize<TDescriptor>(message.Payload);
                var envelope = Convert(serialized, _serializer);
                OnMessageReceived(this, envelope);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ThreadAbortException)
            {
            }
            //catch (Exception) {
            //    throw;
            //}
            finally
            {
                _consumer.CommitOffset(_topic, message.PartitionId.Value, message.Offset, false);
            }
        }


        protected override void Working(CancellationToken cancellationToken)
        {
            var topicMap = new Dictionary<string, int>
            {
                {_topic, 1}
            };
            var streams = _consumer.CreateMessageStreams(topicMap, _decoder);
            var stream = streams[_topic][0];

            while (!cancellationToken.IsCancellationRequested)
                foreach (var message in stream.GetCancellable(cancellationToken)) ProcessingMessage(message);
        }
    }
}