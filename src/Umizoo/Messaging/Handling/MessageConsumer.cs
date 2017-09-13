// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Configurations;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     表示 <typeparamref name="TMessage" /> 的消费程序
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public abstract class MessageConsumer<TMessage> : Consumer<TMessage> where TMessage : IMessage
    {
        private readonly Dictionary<Type, ICollection<HandlerDescriptor>> _envelopeHandlerDescriptors;
        private readonly Dictionary<Type, ICollection<HandlerDescriptor>> _handlerDescriptors;
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
            _handlerDescriptors = new Dictionary<Type, ICollection<HandlerDescriptor>>();
            _envelopeHandlerDescriptors = new Dictionary<Type, ICollection<HandlerDescriptor>>();
            _checkHandlerMode = checkHandlerMode;
        }


        protected override void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     获取类型的处理器集合
        /// </summary>
        protected virtual IEnumerable<HandlerDescriptor> GetHandlerDescriptors(Type messageType)
        {
            var combinedHandlerDescriptors = new List<HandlerDescriptor>();
            if (_handlerDescriptors.ContainsKey(messageType)) combinedHandlerDescriptors.AddRange(_handlerDescriptors[messageType]);

            if (_envelopeHandlerDescriptors.ContainsKey(messageType)) combinedHandlerDescriptors.AddRange(_envelopeHandlerDescriptors[messageType]);

            return combinedHandlerDescriptors;
        }

        /// <summary>
        ///     初始化该类型
        /// </summary>
        protected void Initialize(IObjectContainer container, Type messageType)
        {
            var contractType = typeof(IEnvelopedMessageHandler<>).MakeGenericType(messageType);
            var handlers = container.ResolveAll(contractType).ToArray();
            if (handlers.Length > 0) {
                _envelopeHandlerDescriptors[messageType] = handlers.Select(handler =>
                    new HandlerDescriptor(handler, handlerType => handlerType.GetMethod("Handle", new[] { typeof(Envelope<>).MakeGenericType(messageType) }), HandlerStyle.Senior)).ToArray();

                if (_checkHandlerMode == CheckHandlerMode.OnlyOne) {
                    if (handlers.Length > 1)
                        throw new SystemException(
                            string.Format(
                                "Found more than one handler for this type('{0}') with IEnvelopedMessageHandler<>.",
                                messageType.FullName));

                    return;
                }
            }

            contractType = typeof(IMessageHandler<>).MakeGenericType(messageType);
            handlers = container.ResolveAll(contractType).ToArray();
            if (_checkHandlerMode == CheckHandlerMode.OnlyOne)
                switch (handlers.Length) {
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

            if (handlers.Length > 0) _handlerDescriptors[messageType] = handlers.Select(handler =>
                    new HandlerDescriptor(handler, handlerType => handlerType.GetMethod("Handle", new[] { messageType }), HandlerStyle.Simple)).ToArray();
        }


        protected object Convert(Type messageType, Envelope<TMessage> envelope)
        {
            return Activator.CreateInstance(typeof(Envelope<>).MakeGenericType(messageType), envelope.Body, envelope.MessageId, envelope.Items);
        }

        /// <summary>
        ///     处理消息.
        /// </summary>
        protected override void OnMessageReceived(Envelope<TMessage> envelope)
        {
            var messageType = envelope.Body.GetType();

            var combinedHandlerDescriptors = GetHandlerDescriptors(messageType);

            if (combinedHandlerDescriptors.IsEmpty())
            {
                LogManager.Default.WarnFormat("There is no handler of type('{0}').", messageType.FullName);

                return;
            }

            //foreach (var handler in combinedHandlers)
            //    if (handler is IEnvelopedHandler) TryMultipleInvoke(InvokeHandler, handler, Convert(envelope));
            //    else TryMultipleInvoke(InvokeHandler, handler, envelope.Body);
            foreach(var handlerDescriptor in combinedHandlerDescriptors) {
                switch(handlerDescriptor.HandlerStyle) {
                    case HandlerStyle.Simple:
                        handlerDescriptor.Invode(envelope.Body);
                        break;
                    case HandlerStyle.Senior:
                        handlerDescriptor.Invode(Convert(messageType, envelope));
                        break;
                }
            }
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