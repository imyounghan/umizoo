
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
    using Umizoo.Seeds;

    /// <summary>
    /// The command consumer.
    /// </summary>
    public class CommandConsumer : MessageConsumer<ICommand>
    {
        #region Fields

        private readonly Dictionary<Type, IHandler> _commandHandlers;
        private readonly ICache cache;
        private readonly IMessageBus<IEvent> eventBus;
        private readonly IEventStore eventStore;
        private readonly IMessageBus<IPublishableException> exceptionBus;
        private readonly IRepository repository;
        private readonly IMessageBus<IResult> resultBus;
        private readonly ISnapshotStore snapshotStore;
        private readonly ITextSerializer serializer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConsumer"/> class.
        /// </summary>
        public CommandConsumer(
            IMessageBus<IPublishableException> exceptionBus,
            IMessageBus<IResult> resultBus,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            IRepository repository,
            ICache cache,
            IMessageBus<IEvent> eventBus,
            ITextSerializer serializer,
            IMessageReceiver<Envelope<ICommand>> commandReceiver)
            : base(commandReceiver)
        {
            this.exceptionBus = exceptionBus;
            this.resultBus = resultBus;

            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.repository = repository;
            this.cache = cache;
            this.eventBus = eventBus;
            this.serializer = serializer;

            this._commandHandlers = new Dictionary<Type, IHandler>();

            this.CheckMode = CheckHandlerMode.OnlyOne;
        }

        #endregion

        #region Methods

        public override void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies)
        {
            foreach (var commandType in Configuration.Current.CommandTypes.Values) {
                List<ICommandHandler> commandHandlers =
                container.ResolveAll(typeof(ICommandHandler<>).MakeGenericType(commandType))
                    .OfType<ICommandHandler>()
                    .ToList();
                switch (commandHandlers.Count) {
                    case 0:
                        break;
                    case 1:
                        this._commandHandlers[commandType] = commandHandlers.First();
                        continue;
                    default:
                        throw new SystemException(string.Format(
                                "Found more than one handler for this type('{0}') with ICommandHandler<>.",
                                commandType.FullName));
                }

                this.Initialize(container, commandType);
            }
        }



        protected override void ProcessMessage(Envelope<ICommand> envelope, Type commandType)
        {
            var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];
            IHandler handler;
            if (!this._commandHandlers.TryGetValue(commandType, out handler)) {
                var handlers = this.GetHandlers(commandType);
                if (handlers.IsEmpty()) {
                    var errorMessage = string.Format("The handler of this type('{0}') is not found.", commandType.FullName);
                    if (LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.Debug(errorMessage);
                    }
                    this.NotifyResult(traceInfo, new CommandResult(HandleStatus.Failed, errorMessage));
                    return;
                }
                handler = handlers.FirstOrDefault();
            }

            if (!ConfigurationSettings.CommandFilterEnabled) {
                try {
                    var result = this.ProcessCommand(handler, envelope);
                    this.NotifyResult(traceInfo, result);
                }
                catch (Exception ex) {
                    this.NotifyResult(traceInfo, ex);
                }

                return;
            }



            var handlerContext = new HandlerContext(envelope.Body, handler);
            handlerContext.InvocationContext[StandardMetadata.TraceInfo] = envelope.Items[StandardMetadata.TraceInfo];
            handlerContext.InvocationContext[StandardMetadata.MessageId] = envelope.MessageId;

            IEnumerable<Filter> filters = FilterProviders.Providers.GetFilters(handlerContext);
            var filterInfo = new FilterInfo(filters);

            try {
                ActionExecutedContext postContext = this.InvokeHandlerMethodWithFilters(
                    handlerContext,
                    filterInfo.ActionFilters,
                    envelope);
                if (!postContext.ExceptionHandled) {
                    this.NotifyResult(traceInfo, postContext.ReturnValue ?? postContext.Exception);
                }
                else {
                    this.NotifyResult(traceInfo, postContext.ReturnValue);
                }
            }
            catch (ThreadAbortException) {
                throw;
            }
            catch (Exception ex) {
                ExceptionContext exceptionContext = InvokeExceptionFilters(
                    handlerContext,
                    filterInfo.ExceptionFilters,
                    ex);
                if (!exceptionContext.ExceptionHandled) {
                    this.NotifyResult(traceInfo, exceptionContext.ReturnValue ?? exceptionContext.Exception);
                }
                else {
                    this.NotifyResult(traceInfo, exceptionContext.ReturnValue);
                }
            }
        }


        private static ExceptionContext InvokeExceptionFilters(
            HandlerContext commandHandlerContext,
            IList<IExceptionFilter> filters,
            Exception exception)
        {
            var context = new ExceptionContext(commandHandlerContext, exception);
            foreach (IExceptionFilter filter in filters.Reverse()) {
                filter.OnException(context);
            }

            return context;
        }

        private static ActionExecutedContext InvokeHandlerMethodFilter(
            IActionFilter filter,
            ActionExecutingContext preContext,
            Func<ActionExecutedContext> continuation)
        {
            filter.OnActionExecuting(preContext);

            if (!preContext.WillExecute) {
                return new ActionExecutedContext(preContext, true, null) { ReturnValue = preContext.ReturnValue };
            }

            bool wasError = false;
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
                if (!postContext.ExceptionHandled) {
                    throw;
                }
            }

            if (!wasError) {
                filter.OnActionExecuted(postContext);
            }

            return postContext;
        }


        private void NotifyResult(TraceInfo traceInfo, object result)
        {
            var commandResult = result as CommandResult;

            // 如果自己设置了返回结果直接发送
            if (commandResult != null) {
                this.resultBus.Send(commandResult, traceInfo);
                return;
            }

            var publishableException = result as IPublishableException;

            // 如果出现了可发布异常则将异常转换成返回结果发送
            if (publishableException != null) {
                this.exceptionBus.Publish(publishableException);

                commandResult = new CommandResult(HandleStatus.Failed,
                    publishableException.Message, publishableException.ErrorCode) {
                    Result = serializer.Serialize(publishableException.Data),
                    ReplyType = CommandReturnMode.CommandExecuted
                };
                this.resultBus.Send(commandResult, traceInfo);
                return;
            }

            var ex = result as Exception;

            // 如果出现了系统异常则将异常转换成返回结果发送
            if (ex != null) {
                commandResult = new CommandResult(HandleStatus.Failed, ex.Message) {
                    Result = serializer.Serialize(ex.Data),
                    ReplyType = CommandReturnMode.CommandExecuted
                };
                this.resultBus.Send(commandResult, traceInfo);
            }
        }

        private void InvokeCommandHandler(IHandler commandHandler, Envelope<ICommand> envelope)
        {
            var context = new CommandContext(
                this.eventBus,
                this.resultBus,
                this.eventStore,
                this.snapshotStore,
                this.repository,
                this.cache);
            context.CommandId = envelope.MessageId;
            context.Command = envelope.Body;
            context.TraceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];

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
                ReturnValue = this.ProcessCommand(handlerContext.Handler, envelope)
            };

            Func<ActionExecutedContext> thunk = filters.Reverse().Aggregate(continuation,
                (next, filter) => () => InvokeHandlerMethodFilter(filter, preContext, next));
            return thunk();
        }

        private object ProcessCommand(IHandler handler, Envelope<ICommand> envelope)
        {
            if (handler is ICommandHandler) {
                this.TryMultipleInvoke(this.InvokeCommandHandler, handler, envelope);
                return null;
            }

            if (handler is IEnvelopedHandler) {
                this.TryMultipleInvoke(this.InvokeHandler, handler, envelope);
            }
            else {
                this.TryMultipleInvoke(this.InvokeHandler, handler, envelope.Body);
            }
            return CommandResult.CommandExecuted;
        }

        #endregion
    }
}