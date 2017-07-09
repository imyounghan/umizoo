
namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Kafka.Client.Consumers;
    using Kafka.Client.Messages;
    using Kafka.Client.Serialization;

    using Umizoo.Infrastructure;

    public abstract class KafkaReceiver<TDescriptor, TMessage> : MessageReceiver<TMessage>
        where TDescriptor : class, IDescriptor
        where TMessage : IMessage
    {
        private ZookeeperConsumerConnector _consumer;
        private readonly IDecoder<Message> _decoder = new DefaultDecoder(); 
        
        protected readonly ITextSerializer _serializer;        
        private readonly string topic;

        public KafkaReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
        {
            this._serializer = serializer;

            this.topic = topicProvider.GetTopic(typeof(TDescriptor));
        }

        protected override void Start()
        {
            _consumer = KafkaUtils.CreateBalancedConsumer(topic);
        }

        protected override void Stop()
        {
            _consumer.Dispose();
            _consumer = null;
        }

        protected abstract Envelope<TMessage> Convert(TDescriptor descriptor);

        private void ProcessingMessage(Message message)
        {
            if(LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat(
                    "Pull a message from kafka on topic('{0}'). offset:{1}, partition:{2}.",
                    topic,
                    message.Offset,
                    message.PartitionId);
            }

            try {
                
                var serialized = _serializer.Deserialize<TDescriptor>(message.Payload);
                var envelope = this.Convert(serialized);
                this.OnMessageReceived(this, envelope);
            }
            catch(OperationCanceledException) {
            }
            catch(ThreadAbortException) {
            }
            //catch (Exception) {
            //    throw;
            //}
            finally {
                _consumer.CommitOffset(topic, message.PartitionId.Value, message.Offset, false);
            }
        }


        protected override void ReceiveMessages(CancellationToken cancellationToken)
        {
            var topicMap = new Dictionary<string, int>() {
                { topic, 1 }
            };
            var streams = _consumer.CreateMessageStreams(topicMap, _decoder);
            var stream = streams[topic][0];

            while(!cancellationToken.IsCancellationRequested)
            {
                foreach(Message message in stream.GetCancellable(cancellationToken)) {

                    this.ProcessingMessage(message);
                }
            }
        }
    }
}
