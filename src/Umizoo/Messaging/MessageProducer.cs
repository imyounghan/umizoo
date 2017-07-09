
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
        private readonly BlockingCollection<Envelope<TMessage>> broker;

        public MessageProducer()
        {
            this.broker = new BlockingCollection<Envelope<TMessage>>();
        }


        /// <summary>
        /// 从队列里取出消息
        /// </summary>
        /// <param name="cancellationToken">通知取消的令牌</param>
        protected override void ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) {
                var envelope = this.broker.Take(cancellationToken);

                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat(
                        "Take an envelope '{0}' from local queue.", envelope);
                }

                this.OnMessageReceived(this, envelope);
            }
        }

        #region IMessageBus<TMessage> 成员

        public virtual void Send(Envelope<TMessage> envelope)
        {
            if (LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat(
                    "Prepare to add an envelope '{0}' in local queue.", envelope);
            }

            bool success = this.broker.TryAdd(envelope, 5000);

            if (!success && LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat(
                    "Failed to Add an envelope '{0}' in local queue.", envelope);
            }
        }

        public virtual void Send(IEnumerable<Envelope<TMessage>> envelopes)
        {
            envelopes.ForEach(this.Send);
        }

        #endregion
    }
}
