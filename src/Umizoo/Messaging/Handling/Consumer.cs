// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public abstract class Consumer<TMessage> : Processor where TMessage : IMessage
    {
        private readonly IMessageReceiver<Envelope<TMessage>> _receiver;
        private readonly ProcessingFlags _processingFlag;

        protected Consumer(IMessageReceiver<Envelope<TMessage>> receiver, ProcessingFlags processingFlag)
        {
            _receiver = receiver;
            _processingFlag = processingFlag;
        }

        protected abstract void OnMessageReceived(Envelope<TMessage> envelope);

        private void SubscribeMessageReceived(object sender, Envelope<TMessage> envelope)
        {
            try {
                OnMessageReceived(envelope);
            }
            catch (Exception ex) {
                LogManager.Default.Error(
                    ex,
                    "An exception happened while handling '{0}' through handler, Error will be ignored and message receiving will continue.",
                    envelope.Body);
            }
        }

        /// <summary>
        ///     启动进程
        /// </summary>
        protected override void Start()
        {
            _receiver.MessageReceived += SubscribeMessageReceived;
            _receiver.Start();

            LogManager.Default.InfoFormat("{0} Consumer Started!", _processingFlag);
        }

        /// <summary>
        ///     停止进程
        /// </summary>
        protected override void Stop()
        {
            _receiver.MessageReceived -= SubscribeMessageReceived;
            _receiver.Stop();

            LogManager.Default.InfoFormat("{0} Consumer Stopped!", _processingFlag);
        }
    }
}