// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     表示 <typeparamref name="TMessage" /> 的消费程序
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public abstract class MessageConsumer<TMessage> : Consumer<TMessage> where TMessage : IMessage
    {
        private readonly Dictionary<Type, ICollection<IHandler>> _envelopeHandlers;
        private readonly Dictionary<Type, ICollection<IHandler>> _handlers;
        private readonly CheckHandlerMode _checkHandlerMode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageConsumer{TMessage}" /> class.
        /// </summary>
        protected MessageConsumer(IMessageReceiver<Envelope<TMessage>> receiver, ProcessingFlags processingFlag)
            : this(receiver, CheckHandlerMode.Ignored, processingFlag)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageConsumer{TMessage}" /> class.
        /// </summary>
        protected MessageConsumer(IMessageReceiver<Envelope<TMessage>> receiver, CheckHandlerMode checkHandlerMode,
            ProcessingFlags processingFlag)
            : base(receiver, processingFlag)
        {
            _handlers = new Dictionary<Type, ICollection<IHandler>>();
            _envelopeHandlers = new Dictionary<Type, ICollection<IHandler>>();
            _checkHandlerMode = checkHandlerMode;
        }


        protected override void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     获取类型的处理器集合
        /// </summary>
        protected IEnumerable<IHandler> GetHandlers(Type messageType)
        {
            var combinedHandlers = new List<IHandler>();
            if (_handlers.ContainsKey(messageType)) combinedHandlers.AddRange(_handlers[messageType]);

            if (_envelopeHandlers.ContainsKey(messageType)) combinedHandlers.AddRange(_envelopeHandlers[messageType]);

            return combinedHandlers;
        }

        /// <summary>
        ///     初始化该类型
        /// </summary>
        protected void Initialize(IObjectContainer container, Type messageType)
        {
            var envelopedEventHandlers =
                container.ResolveAll(typeof(IEnvelopedMessageHandler<>).MakeGenericType(messageType))
                    .OfType<IEnvelopedHandler>()
                    .Cast<IHandler>()
                    .ToList();
            if (envelopedEventHandlers.Count > 0)
            {
                _envelopeHandlers[messageType] = envelopedEventHandlers;

                if (_checkHandlerMode == CheckHandlerMode.OnlyOne)
                {
                    if (envelopedEventHandlers.Count > 1)
                        throw new SystemException(
                            string.Format(
                                "Found more than one handler for this type('{0}') with IEnvelopedMessageHandler<>.",
                                messageType.FullName));

                    return;
                }
            }

            var handlers =
                container.ResolveAll(typeof(IMessageHandler<>).MakeGenericType(messageType)).OfType<IHandler>()
                    .ToList();
            if (_checkHandlerMode == CheckHandlerMode.OnlyOne)
                switch (handlers.Count)
                {
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

            if (handlers.Count > 0) _handlers[messageType] = handlers;
        }

        protected Envelope Convert(Envelope<TMessage> envelope)
        {
            var resultType = envelope.Body.GetType();

            return (Envelope)Activator.CreateInstance(typeof(Envelope<>).MakeGenericType(resultType), envelope.Body, envelope.MessageId);
        }

        /// <summary>
        ///     处理消息.
        /// </summary>
        protected override void OnMessageReceived(Envelope<TMessage> envelope)
        {
            var messageType = envelope.Body.GetType();

            var combinedHandlers = GetHandlers(messageType);

            if (combinedHandlers.IsEmpty())
            {
                LogManager.Default.WarnFormat("There is no handler of type('{0}').", messageType.FullName);

                return;
            }

            foreach (var handler in combinedHandlers)
                if (handler is IEnvelopedHandler) TryMultipleInvoke(InvokeHandler, handler, Convert(envelope));
                else TryMultipleInvoke(InvokeHandler, handler, envelope.Body);
        }

        protected void TryMultipleInvoke<THandler, TParameter>(
            Action<THandler, TParameter> retryAction,
            THandler handler,
            TParameter parameter)
        {
            var retryTimes = ConfigurationSettings.HandleRetrytimes;
            var retryInterval = ConfigurationSettings.HandleRetryInterval;

            var count = 0;
            while (count++ < retryTimes)
                try
                {
                    retryAction(handler, parameter);
                    break;
                }
                catch (ApplicationException ex)
                {
                    LogManager.Default.Error(
                        ex,
                        "ApplicationException raised when handling '{0}' on '{1}', exit retry and throw.",
                        parameter,
                        handler.GetType().FullName);
                    throw ex;
                }
                catch (SystemException ex)
                {
                    LogManager.Default.Error(
                        ex,
                        "SystemException raised when handling '{0}' on '{1}', exit retry and throw.",
                        parameter,
                        handler.GetType().FullName);
                    throw ex;
                }
                catch (Exception ex)
                {
                    if (count == retryTimes)
                    {
                        LogManager.Default.Error(
                            ex,
                            "Exception raised when handling '{0}' on '{1}', the retry count has been reached.",
                            parameter,
                            handler.GetType().FullName);
                        throw ex;
                    }

                    Thread.Sleep(retryInterval);
                }

            if (LogManager.Default.IsDebugEnabled)
                LogManager.Default.DebugFormat("Handle '{0}' on '{1}' successfully.",
                    parameter,
                    handler.GetType().FullName);
        }

        protected void InvokeHandler<THandler, TParameter>(THandler handler, TParameter parameter)
        {
            ((dynamic)handler).Handle((dynamic)parameter);
        }

        /// <summary>
        ///     检查处理器的方式
        /// </summary>
        protected enum CheckHandlerMode
        {
            /// <summary>
            ///     The only one.
            /// </summary>
            OnlyOne,

            /// <summary>
            ///     The ignored.
            /// </summary>
            Ignored
        }
    }
}