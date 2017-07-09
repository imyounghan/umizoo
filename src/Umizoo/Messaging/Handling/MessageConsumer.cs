// --------------------------------------------------------------------------------------------------------------------

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Composition;

    /// <summary>
    /// 表示 <typeparamref name="TMessage"/> 的消费程序
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public abstract class MessageConsumer<TMessage> : Processor, IInitializer
    {
        #region Fields

        private readonly Dictionary<Type, ICollection<IHandler>> _envelopeHandlers;
        private readonly Dictionary<Type, ICollection<IHandler>> _handlers;
        private readonly IMessageReceiver<Envelope<TMessage>> receiver;
        /// <summary>
        /// 消息者名称
        /// </summary>
        private readonly string messageTypeName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageConsumer{TMessage}"/> class.
        /// </summary>
        public MessageConsumer(IMessageReceiver<Envelope<TMessage>> receiver)
            : this(receiver, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageConsumer{TMessage}"/> class.
        /// </summary>
        protected MessageConsumer(IMessageReceiver<Envelope<TMessage>> receiver, string messageTypeName)
        {
            this._handlers = new Dictionary<Type, ICollection<IHandler>>();
            this._envelopeHandlers = new Dictionary<Type, ICollection<IHandler>>();

            this.messageTypeName = messageTypeName.IfEmpty(() => typeof(TMessage).Name.Substring(1));
            this.receiver = receiver;
            this.CheckMode = CheckHandlerMode.Ignored;
        }

        #endregion

        #region Enums

        /// <summary>
        /// 检查处理器的方式
        /// </summary>
        protected enum CheckHandlerMode
        {
            /// <summary>
            /// The only one.
            /// </summary>
            OnlyOne,

            /// <summary>
            /// The ignored.
            /// </summary>
            Ignored,
        }

        #endregion

        #region Properties

        /// <summary>
        /// 设置或获取检查处理器的方式
        /// </summary>
        protected CheckHandlerMode CheckMode { get; set; }

        #endregion

        #region Methods and Operators

        /// <summary>
        /// 初始化消费者程序
        /// </summary>
        public abstract void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies);

        protected override void Dispose(bool disposing)
        { }

        /// <summary>
        /// 获取类型的处理器集合
        /// </summary>
        protected IEnumerable<IHandler> GetHandlers(Type messageType)
        {
            var combinedHandlers = new List<IHandler>();
            if (this._handlers.ContainsKey(messageType)) {
                combinedHandlers.AddRange(this._handlers[messageType]);
            }

            if (this._envelopeHandlers.ContainsKey(messageType)) {
                combinedHandlers.AddRange(this._envelopeHandlers[messageType]);
            }

            return combinedHandlers;
        }

        /// <summary>
        /// 初始化该类型
        /// </summary>
        protected void Initialize(IObjectContainer container, Type messageType)
        {
            List<IHandler> envelopedEventHandlers =
                container.ResolveAll(typeof(IEnvelopedMessageHandler<>).MakeGenericType(messageType))
                    .OfType<IEnvelopedHandler>()
                    .Cast<IHandler>()
                    .ToList();
            if (envelopedEventHandlers.Count > 0) {
                this._envelopeHandlers[messageType] = envelopedEventHandlers;

                if (this.CheckMode == CheckHandlerMode.OnlyOne) {
                    if (envelopedEventHandlers.Count > 1) {
                        throw new SystemException(
                            string.Format(
                                "Found more than one handler for this type('{0}') with IEnvelopedMessageHandler<>.",
                                messageType.FullName));
                    }

                    return;
                }
            }

            List<IHandler> handlers =
                container.ResolveAll(typeof(IMessageHandler<>).MakeGenericType(messageType)).OfType<IHandler>().ToList();
            if (this.CheckMode == CheckHandlerMode.OnlyOne) {
                switch (handlers.Count) {
                    case 0:
                        throw new SystemException(
                            string.Format("The handler of this type('{0}') is not found.", messageType.FullName));
                    case 1:
                        break;
                    default:
                        throw new SystemException(
                            string.Format(
                                "Found more than one handler for '{0}' with IMessageHandler<>.",
                                messageType.FullName));
                }
            }

            if (handlers.Count > 0) {
                this._handlers[messageType] = handlers;
            }
        }

        /// <summary>
        /// 当收到消息后的处理方法
        /// </summary>
        /// <param name="sender">发送程序</param>
        /// <param name="envelope">一个消息</param>
        private void OnMessageReceived(object sender, Envelope<TMessage> envelope)
        {
            try {
                this.ProcessMessage(envelope, envelope.Body.GetType());
            }
            catch (Exception ex) {
                LogManager.Default.Error(
                        ex,
                        "An exception happened while handling '{0}' through handler, Error will be ignored and message receiving will continue.",
                        envelope.Body);
            }
        }


        /// <summary>
        /// 处理消息.
        /// </summary>
        protected virtual void ProcessMessage(Envelope<TMessage> envelope, Type messageType)
        {
            IEnumerable<IHandler> combinedHandlers = this.GetHandlers(messageType);

            if (combinedHandlers.IsEmpty()) {
                if (LogManager.Default.IsWarnEnabled) {
                    LogManager.Default.WarnFormat("There is no handler of type('{0}').", messageType.FullName);
                }

                return;
            }

            foreach (IHandler handler in combinedHandlers) {
                if (handler is IEnvelopedHandler) {
                    this.TryMultipleInvoke(this.InvokeHandler, handler, envelope);
                }
                else {
                    this.TryMultipleInvoke(this.InvokeHandler, handler, envelope.Body);
                }
            }
        }

        /// <summary>
        ///     启动进程
        /// </summary>
        protected override void Start()
        {
            this.receiver.MessageReceived += this.OnMessageReceived;
            this.receiver.Start();

            LogManager.Default.InfoFormat("{0} Consumer Started!", this.messageTypeName);
        }

        /// <summary>
        ///     停止进程
        /// </summary>
        protected override void Stop()
        {
            this.receiver.MessageReceived -= this.OnMessageReceived;
            this.receiver.Stop();

            LogManager.Default.InfoFormat("{0} Consumer Stopped!", this.messageTypeName);
        }

        protected void TryMultipleInvoke<THandler, TParameter>(
            Action<THandler, TParameter> retryAction,
            THandler handler,
            TParameter parameter)
        {
            int retryTimes = ConfigurationSettings.HandleRetrytimes;
            int retryInterval = ConfigurationSettings.HandleRetryInterval;

            int count = 0;
            while (count++ < retryTimes) {
                try {
                    retryAction(handler, parameter);
                    break;
                }
                catch (UnrecoverableException ex) {
                    LogManager.Default.Error(
                        ex,
                        "UnrecoverableException raised when handling '{0}' on '{1}', exit retry and throw.",
                        parameter,
                        handler.GetType().FullName);
                    throw ex;
                }
                catch (Exception ex) {
                    if (count == retryTimes) {
                        LogManager.Default.Error(
                                ex,
                                "Exception raised when handling '{0}' on '{1}', the retry count has been reached.",
                                parameter,
                                handler.GetType().FullName);
                        throw ex;
                    }

                    Thread.Sleep(retryInterval);
                }
            }

            if (LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat("Handle '{0}' on '{1}' successfully.",
                    parameter, 
                    handler.GetType().FullName);
            }
        }

        protected void InvokeHandler<THandler, TParameter>(THandler handler, TParameter parameter)
        {
            ((dynamic)handler).Handle((dynamic)parameter);
        }

        #endregion
    }
}