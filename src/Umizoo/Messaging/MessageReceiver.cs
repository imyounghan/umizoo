// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


using System;
using System.Threading;
using System.Threading.Tasks;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging
{
    public abstract class MessageReceiver<TMessage> : IMessageReceiver<Envelope<TMessage>>
        where TMessage : IMessage
    {
        /// <summary>
        ///     通知源
        /// </summary>
        private CancellationTokenSource _cancellationSource;

        


        private event EventHandler<Envelope<TMessage>> messageReceived = (sender, args) => { };

        protected virtual void OnMessageReceived(object sender, Envelope<TMessage> e)
        {
            try
            {
                messageReceived.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex, "Take an envelope '{0}' from local queue.", e);
            }
        }

        protected virtual void Start()
        {
        }

        protected virtual void Stop()
        {
        }

        event EventHandler<Envelope<TMessage>> IMessageReceiver<Envelope<TMessage>>.MessageReceived
        {
            add { messageReceived += value; }
            remove { messageReceived -= value; }
        }

        void IMessageReceiver<Envelope<TMessage>>.Start()
        {
            if (_cancellationSource == null)
            {
                _cancellationSource = new CancellationTokenSource();
                Task.Factory.StartNew(ContinuousWorking,
                    _cancellationSource.Token,
                    TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness,
                    TaskScheduler.Current);
            }
        }

        void IMessageReceiver<Envelope<TMessage>>.Stop()
        {
            if (_cancellationSource != null)
            {
                using (_cancellationSource)
                {
                    _cancellationSource.Cancel();
                    _cancellationSource = null;
                }
            }
        }

        protected abstract void Working(CancellationToken cancellationToken);

        private void ContinuousWorking()
        {
            Working(_cancellationSource.Token);
        }
    }
}