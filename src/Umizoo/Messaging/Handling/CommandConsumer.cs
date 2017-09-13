// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Configurations;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Composition.Interception;
using Umizoo.Seeds;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     The command consumer.
    /// </summary>
    public class CommandConsumer : MessageConsumer<ICommand>, IInitializer
    {
        private readonly Dictionary<Type, HandlerDescriptor> _commandHandlerDescriptors;
        private readonly IMessageBus<IPublishableException> _exceptionBus;
        private readonly IRepository _repository;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly ITextSerializer _serializer;
        private readonly HandlerPipelineManager _pipelineManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandConsumer" /> class.
        /// </summary>
        public CommandConsumer(
            IMessageBus<IPublishableException> exceptionBus,
            IMessageBus<IResult> resultBus,
            IRepository repository,
            ITextSerializer serializer,
            IMessageReceiver<Envelope<ICommand>> commandReceiver)
            : base(commandReceiver, CheckHandlerMode.OnlyOne, ProcessingFlags.Command)
        {
            _exceptionBus = exceptionBus;
            _resultBus = resultBus;
            _repository = repository;
            _serializer = serializer;

            _commandHandlerDescriptors = new Dictionary<Type, HandlerDescriptor>();
            _pipelineManager = new HandlerPipelineManager();
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            BasicTypes.CommandTypes.Values.ForEach(commandType => {
                var commandHandlers =
                    container.ResolveAll(typeof(ICommandHandler<>).MakeGenericType(commandType)).ToList();
                switch (commandHandlers.Count) {
                    case 0:
                        break;
                    case 1:
                        _commandHandlerDescriptors[commandType] = new HandlerDescriptor(commandHandlers[0], handlerType => handlerType.GetMethod("Handle", new[] { typeof(ICommandContext), commandType }), HandlerStyle.Special);
                        break;
                    default:
                        throw new SystemException(string.Format(
                            "Found more than one handler for this type('{0}') with ICommandHandler<>.",
                            commandType.FullName));
                }
                if (commandHandlers.Count == 0) {
                    Initialize(container, commandType);
                }
                var handlerDescriptor = GetHandlerDescriptors(commandType).First();
                var callHandlers = HandlerAttributeHelper.GetHandlersFor(handlerDescriptor.Method, container);
                _pipelineManager.InitializePipeline(handlerDescriptor.Method, callHandlers);
            });
        }

        protected override IEnumerable<HandlerDescriptor> GetHandlerDescriptors(Type messageType)
        {
            HandlerDescriptor handlerDescriptor;
            if (_commandHandlerDescriptors.TryGetValue(messageType, out handlerDescriptor)) {
                yield return handlerDescriptor;
            }

            yield return base.GetHandlerDescriptors(messageType).First();
        }

        private object[] CreateParameters(HandlerStyle handlerStyle, Type commandType, Envelope<ICommand> envelope)
        {
            switch (handlerStyle) {
                case HandlerStyle.Special:
                    return new object[] { new CommandContext(_resultBus, _repository, envelope), envelope.Body };
                case HandlerStyle.Senior:
                    return new object[] { Convert(commandType, envelope) };
                case HandlerStyle.Simple:
                    return new object[] { envelope.Body };
                default:
                    return new object[0];
            }
        }

        protected override void OnMessageReceived(Envelope<ICommand> envelope)
        {
            var commandType = envelope.Body.GetType();
            var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];
            var handlerDescriptor = GetHandlerDescriptors(commandType).First();


            var methodInvocation = new MethodInvocation(handlerDescriptor.Target, handlerDescriptor.Method, CreateParameters(handlerDescriptor.HandlerStyle, commandType, envelope));
            methodInvocation.InvocationContext[StandardMetadata.TraceInfo] = traceInfo;
            methodInvocation.InvocationContext[StandardMetadata.MessageId] = envelope.MessageId;

            try {
                var methodReturn = _pipelineManager.GetPipeline(handlerDescriptor.Method).Invoke(methodInvocation,
                    delegate (IMethodInvocation input, GetNextHandlerDelegate getNext) {
                        handlerDescriptor.Invode(input.Arguments.Cast<object>().ToArray());
                        if(handlerDescriptor.HandlerStyle == HandlerStyle.Special) {
                            ((CommandContext)input.Arguments[0]).Commit();
                            return input.CreateMethodReturn(CommandResult.CommandExecuted);
                        }
                        return input.CreateMethodReturn(null);
                    });
                NotifyResult(traceInfo, methodReturn.Exception ?? methodReturn.ReturnValue);
            }
            catch (Exception ex) {
                NotifyResult(traceInfo, ex);
            }
        }


        private void NotifyResult(TraceInfo traceInfo, object result)
        {
            var commandResult = result as ICommandResult;

            // 如果自己设置了返回结果直接发送
            if (commandResult != null) {
                _resultBus.Send(commandResult, traceInfo);
                return;
            }

            var publishableException = result as IPublishableException;

            // 如果出现了可发布异常则将异常转换成返回结果发送
            if (publishableException != null) {
                _exceptionBus.Publish(publishableException);

                commandResult = new CommandResult(HandleStatus.Failed,
                    publishableException.Message, publishableException.ErrorCode) {
                    Result = _serializer.Serialize(publishableException.Data),
                    ReplyType = CommandReturnMode.CommandExecuted
                };
                _resultBus.Send(commandResult, traceInfo);
                return;
            }

            var ex = result as Exception;

            // 如果出现了系统异常则将异常转换成返回结果发送
            if (ex != null) {
                commandResult = new CommandResult(HandleStatus.Failed, ex.Message) {
                    Result = _serializer.Serialize(ex.Data),
                    ReplyType = CommandReturnMode.CommandExecuted
                };
                _resultBus.Send(commandResult, traceInfo);
            }
        }
    }
}