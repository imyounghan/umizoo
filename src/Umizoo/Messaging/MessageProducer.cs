// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    

    public class MessageProducer<TMessage> : MessageReceiver<TMessage>, IMessageBus<TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// 消息队列
        /// </summary>
        private readonly BlockingCollection<Envelope<TMessage>> _broker;

        public MessageProducer()
        {
            _broker = new BlockingCollection<Envelope<TMessage>>();
        }


        protected override void Working(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var envelope = _broker.Take(cancellationToken);

                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat("Take an envelope '{0}' from local queue.", envelope);
                }

                OnMessageReceived(this, envelope);
            }
        }


        public void Send(Envelope<TMessage> envelope)
        {
            if (LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat(
                    "Prepare to add an envelope '{0}' in local queue.", envelope);
            }

            bool success = _broker.TryAdd(envelope, 5000);

            if (!success && LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat(
                    "Failed to Add an envelope '{0}' in local queue.", envelope);
            }
        }

        public void Send(IEnumerable<Envelope<TMessage>> envelopes)
        {
            envelopes.ForEach(Send);
        }
    }
}
