// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Filtering;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;
using Umizoo.Infrastructure;
using Umizoo.Seeds;

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    ///     The command consumer.
    /// </summary>
    public class CommandConsumer : MessageConsumer<ICommand>, IInitializer
    {
        private readonly Dictionary<Type, IHandler> _commandHandlers;
        private readonly IMessageBus<IPublishableException> _exceptionBus;
        private readonly IRepository _repository;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly ITextSerializer _serializer;

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

            _commandHandlers = new Dictionary<Type, IHandler>();
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            types.Where(type => type.IsClass && !type.IsAbstract && typeof(ICommand).IsAssignableFrom(type))
                .ForEach(commandType => {
                    var commandHandlers =
                        container.ResolveAll(typeof(ICommandHandler<>).MakeGenericType(commandType))
                            .OfType<ICommandHandler>()
                            .ToList();
                    switch (commandHandlers.Count) {
                        case 0:
                            break;
                        case 1:
                            _commandHandlers[commandType] = commandHandlers.First();
                            return;
                        default:
                            throw new SystemException(string.Format(
                                "Found more than one handler for this type('{0}') with ICommandHandler<>.",
                                commandType.FullName));
                    }

                    Initialize(container, commandType);
                });
        }


        protected override void OnMessageReceived(Envelope<ICommand> envelope)
        {
            var commandType = envelope.Body.GetType();
            var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];
            IHandler handler;
            if (!_commandHandlers.TryGetValue(commandType, out handler)) {
                var handlers = GetHandlers(commandType);
                if (handlers.IsEmpty()) {
                    var errorMessage = string.Format("The handler of this type('{0}') is not found.",
                        commandType.FullName);
                    if (LogManager.Default.IsDebugEnabled) LogManager.Default.Debug(errorMessage);
                    NotifyResult(traceInfo, new CommandResult(HandleStatus.Failed, errorMessage));
                    return;
                }
                handler = handlers.FirstOrDefault();
            }

            if (!ConfigurationSettings.CommandFilterEnabled) {
                try {
                    var result = ProcessCommand(handler, envelope);
                    NotifyResult(traceInfo, result);
                }
                catch (Exception ex) {
                    NotifyResult(traceInfo, ex);
                }

                return;
            }


            var handlerContext = new HandlerContext(envelope.Body, handler);
            handlerContext.InvocationContext[StandardMetadata.TraceInfo] = envelope.Items[StandardMetadata.TraceInfo];
            handlerContext.InvocationContext[StandardMetadata.MessageId] = envelope.MessageId;

            var filters = FilterProviders.Providers.GetFilters(handlerContext);
            var filterInfo = new FilterInfo(filters);

            try {
                var postContext = InvokeHandlerMethodWithFilters(
                    handlerContext,
                    filterInfo.ActionFilters,
                    envelope);
                if (!postContext.ExceptionHandled)
                    NotifyResult(traceInfo, postContext.ReturnValue ?? postContext.Exception);
                else NotifyResult(traceInfo, postContext.ReturnValue);
            }
            catch (ThreadAbortException) {
                throw;
            }
            catch (Exception ex) {
                var exceptionContext = InvokeExceptionFilters(
                    handlerContext,
                    filterInfo.ExceptionFilters,
                    ex);
                if (!exceptionContext.ExceptionHandled)
                    NotifyResult(traceInfo, exceptionContext.ReturnValue ?? exceptionContext.Exception);
                else NotifyResult(traceInfo, exceptionContext.ReturnValue);
            }
        }


        private static ExceptionContext InvokeExceptionFilters(
            HandlerContext commandHandlerContext,
            IList<IExceptionFilter> filters,
            Exception exception)
        {
            var context = new ExceptionContext(commandHandlerContext, exception);
            foreach (var filter in filters.Reverse()) filter.OnException(context);

            return context;
        }

        private static ActionExecutedContext InvokeHandlerMethodFilter(
            IActionFilter filter,
            ActionExecutingContext preContext,
            Func<ActionExecutedContext> continuation)
        {
            filter.OnActionExecuting(preContext);

            if (!preContext.WillExecute)
                return new ActionExecutedContext(preContext, true, null) { ReturnValue = preContext.ReturnValue };

            var wasError = false;
            ActionExecutedContext postContext = null;

            try {
                postContext = continuation();
            }
            catch (ThreadAbortException) {
                postContext = new ActionExecutedContext(preContext, false /* canceled */, null /* exception */);
                filter.OnActionExecuted(postContext);
                throw;
            }
            catch (Exception ex) {
                wasError = true;
                postContext = new ActionExecutedContext(preContext, false, ex);
                filter.OnActionExecuted(postContext);
                if (!postContext.ExceptionHandled) throw;
            }

            if (!wasError) filter.OnActionExecuted(postContext);

            return postContext;
        }


        private void NotifyResult(TraceInfo traceInfo, object result)
        {
            var commandResult = result as CommandResult;

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

        private void InvokeCommandHandler(IHandler commandHandler, Envelope<ICommand> envelope)
        {
            var context = new CommandContext(_resultBus, _repository, envelope);

            ((dynamic)commandHandler).Handle((dynamic)context, (dynamic)envelope.Body);

            context.Commit();
        }

        private ActionExecutedContext InvokeHandlerMethodWithFilters(
            HandlerContext handlerContext,
            IList<IActionFilter> filters,
            Envelope<ICommand> envelope)
        {
            var preContext = new ActionExecutingContext(handlerContext);

            Func<ActionExecutedContext> continuation = () => new ActionExecutedContext(handlerContext, false, null) {
                ReturnValue = ProcessCommand(handlerContext.Handler, envelope)
            };

            var thunk = filters.Reverse().Aggregate(continuation,
                (next, filter) => () => InvokeHandlerMethodFilter(filter, preContext, next));
            return thunk();
        }

        private object ProcessCommand(IHandler handler, Envelope<ICommand> envelope)
        {
            if (handler is ICommandHandler) {
                TryMultipleInvoke(InvokeCommandHandler, handler, envelope);
                return null;
            }

            if (handler is IEnvelopedHandler)
                TryMultipleInvoke(InvokeHandler, handler, Convert(envelope));
            else
                TryMultipleInvoke(InvokeHandler, handler, envelope.Body);
            return CommandResult.CommandExecuted;
        }
    }
}