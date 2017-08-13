using System;
using System.Collections.Generic;
using System.Threading;
using Kafka.Client.Cfg;
using Kafka.Client.Consumers;
using Kafka.Client.Helper;
using Kafka.Client.Messages;
using Kafka.Client.Producers;
using Kafka.Client.Requests;

namespace Umizoo.Infrastructure
{
    public class KafkaUtils
    {
        public static readonly Lazy<ZooKeeperConfiguration> ZooKeeper =
            new Lazy<ZooKeeperConfiguration>(CreateZooKeeper, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ZooKeeperConfiguration CreateZooKeeper()
        {
            return new ZooKeeperConfiguration(ZooKeeperSetting.Address, 3000, 4000, 8000);
        }

        public static Producer CreateProducer()
        {
            var producerConfiguration = new ProducerConfiguration(new List<BrokerConfiguration>())
            {
                AckTimeout = 30000,
                RequiredAcks = -1,
                ZooKeeper = ZooKeeper.Value
            };

            return new Producer(producerConfiguration);
        }

        public static ZookeeperConsumerConnector CreateBalancedConsumer(string key)
        {
            var consumerConfiguration = new ConsumerConfiguration
            {
                AutoCommit = false,
                GroupId = "group_umizoo",
                //ConsumerId = string.Format("consumer_{0}_{1}", key, ConfigurationSettings.InnerAddress),
                BufferSize = ConsumerConfiguration.DefaultBufferSize,
                MaxFetchBufferLength = ConsumerConfiguration.DefaultMaxFetchBufferLength,
                FetchSize = ConsumerConfiguration.DefaultFetchSize,
                AutoOffsetReset = OffsetRequest.SmallestTime,
                ZooKeeper = ZooKeeper.Value
            };

            return new ZookeeperConsumerConnector(consumerConfiguration, true);
        }

        public static void CreateTopicIfNotExists(string topic)
        {
            if (!TopicExsits(topic)) CreateTopic(topic);
        }

        private static bool TopicExsits(string topic)
        {
            var managerConfig = new KafkaSimpleManagerConfiguration
            {
                FetchSize = KafkaSimpleManagerConfiguration.DefaultFetchSize,
                BufferSize = KafkaSimpleManagerConfiguration.DefaultBufferSize,
                Zookeeper = ZooKeeperSetting.Address
            };
            using (var kafkaManager = new KafkaSimpleManager<int, Message>(managerConfig))
            {
                try
                {
                    var allPartitions = kafkaManager.GetTopicPartitionsFromZK(topic);
                    return allPartitions.Count > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private static void CreateTopic(string topic)
        {
            using (var producer = CreateProducer())
            {
                try
                {
                    var data = new ProducerData<string, Message>(topic, string.Empty, new Message(new byte[0]));
                    producer.Send(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Create topic {0} failed. exception:{1}", topic, ex);
                }
            }
        }
    }
}