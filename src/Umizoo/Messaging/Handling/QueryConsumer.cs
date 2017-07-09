

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Composition;

    public class QueryConsumer : Processor, IInitializer
    {
        private readonly Dictionary<Type, IHandler> _handlers;
        private readonly Dictionary<Type, Type> _queryToResultMap;
        private readonly IMessageReceiver<Envelope<IQuery>> receiver;
        private readonly IMessageBus<IResult> resultBus;
        private readonly SemaphoreSlim semaphore;


        public QueryConsumer(IMessageReceiver<Envelope<IQuery>> receiver,
            IMessageBus<IResult> resultBus)
        {
            this._handlers = new Dictionary<Type, IHandler>();
            this._queryToResultMap = new Dictionary<Type, Type>();
            this.receiver = receiver;
            this.resultBus = resultBus;

            var threadCount = ConfigurationSettings.ParallelQueryThead;
            if (threadCount > 1) {
                this.semaphore = new SemaphoreSlim(threadCount, threadCount);
            }
        }

        protected override void Dispose(bool disposing)
        {
        }

        private void OnMessageReceived(object sender, Envelope<IQuery> envelope)
        {
            var queryType = envelope.Body.GetType();

            var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];

            IHandler handler;
            if (!this._handlers.TryGetValue(queryType, out handler)) {
                var errorMessage = string.Format("The handler of this type('{0}') is not found.", queryType.FullName);
                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.Debug(errorMessage);
                }
                this.NotifyResult(traceInfo, new QueryResult(HandleStatus.Failed, errorMessage));
                return;
            }


            if (ConfigurationSettings.ParallelQueryThead > 1) {
                semaphore.Wait();
                Task.Factory.StartNew(() => {
                    this.ExecutingQuery(handler, envelope.Body, traceInfo);
                    semaphore.Release();
                });
                return;
            }

            this.ExecutingQuery(handler, envelope.Body, traceInfo);
        }

        private void ExecutingQuery(IHandler handler, IQuery query, TraceInfo traceInfo)
        {
            if (!ConfigurationSettings.QueryFilterEnabled) {
                try {
                    var result = this.Execute(handler, query);
                    this.NotifyResult(traceInfo, result);
                }
                catch (Exception ex) {
                    this.NotifyResult(traceInfo, ex);
                }

                return;
            }


            var handlerContext = new HandlerContext(query, handler);
            handlerContext.InvocationContext[StandardMetadata.MessageId] = traceInfo.Id;

            IEnumerable<Filter> filters = FilterProviders.Providers.GetFilters(handlerContext);
            var filterInfo = new FilterInfo(filters);

            try {
                ActionExecutedContext postContext = this.InvokeHandlerMethodWithFilters(
                    handlerContext,
                    filterInfo.ActionFilters,
                    query);
                if (!postContext.ExceptionHandled) {
                    this.NotifyResult(traceInfo, postContext.ReturnValue ?? postContext.Exception);
                }
                else {
                    this.NotifyResult(traceInfo, postContext.ReturnValue);
                }
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

        private void NotifyResult(TraceInfo traceInfo, object result)
        {
            var ex = result as Exception;

            QueryResult queryResult;

            // 如果出现了系统异常则将异常转换成返回结果发送
            if (ex != null) {
                queryResult = new QueryResult(HandleStatus.Failed, ex.Message);
                this.resultBus.Send(queryResult, traceInfo);
                return;
            }

            if (result == null || result == DBNull.Value) {
                queryResult = new QueryResult(HandleStatus.Nothing);
                this.resultBus.Send(queryResult, traceInfo);
                return;
            }

            IEnumerable enumerable = result as IEnumerable;
            if (enumerable != null && !enumerable.GetEnumerator().MoveNext()) {
                queryResult = new QueryResult(HandleStatus.Nothing);
            }
            else {
                queryResult = new QueryResult(result);
                
            }
            this.resultBus.Send(queryResult, traceInfo);
        }

        private static ExceptionContext InvokeExceptionFilters(
           HandlerContext queryHandlerContext,
           IList<IExceptionFilter> filters,
           Exception exception)
        {
            var context = new ExceptionContext(queryHandlerContext, exception);
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
            ActionExecutedContext postContext;

            try {
                postContext = continuation();
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

        private ActionExecutedContext InvokeHandlerMethodWithFilters(
            HandlerContext handlerContext,
            IList<IActionFilter> filters,
            IQuery query)
        {
            var preContext = new ActionExecutingContext(handlerContext);

            Func<ActionExecutedContext> continuation = () => new ActionExecutedContext(handlerContext, false, null) {
                ReturnValue = this.Execute(handlerContext.Handler, query)
            };

            Func<ActionExecutedContext> thunk = filters.Reverse().Aggregate(continuation,
                (next, filter) => () => InvokeHandlerMethodFilter(filter, preContext, next));
            return thunk();
        }

        private object Execute(IHandler handler, IQuery query)
        {
            int retryTimes = ConfigurationSettings.HandleRetrytimes;
            int retryInterval = ConfigurationSettings.HandleRetryInterval;

            int count = 0;
            object result = null;
            while (count++ < retryTimes) {
                try {
                    result = ((dynamic)handler).Handle((dynamic)query);
                    break;
                }
                catch (Exception ex) {
                    if (count == retryTimes) {
                        LogManager.Default.Error(
                               ex,
                               "Exception raised when handling '{0}({1})' on '{2}', the retry count has been reached.",
                               query.GetType().FullName,
                               DefaultTextSerializer.Instance.Serialize(query),
                               handler.GetType().FullName);
                        throw ex;
                    }

                    Thread.Sleep(retryInterval);
                }
            }

            if (LogManager.Default.IsDebugEnabled) {
                LogManager.Default.DebugFormat("Handle '{0}({1})' on '{2}' successfully.",
                    query.GetType().FullName,
                    DefaultTextSerializer.Instance.Serialize(query),
                    handler.GetType().FullName);
            }

            return result;
        }

        /// <summary>
        ///     启动进程
        /// </summary>
        protected override void Start()
        {
            this.receiver.MessageReceived += this.OnMessageReceived;
            this.receiver.Start();

            LogManager.Default.InfoFormat("Query Consumer Started!");
        }

        /// <summary>
        ///     停止进程
        /// </summary>
        protected override void Stop()
        {
            this.receiver.MessageReceived -= this.OnMessageReceived;
            this.receiver.Stop();

            LogManager.Default.InfoFormat("Query Consumer Stopped!");
        }

        #region IInitializer 成员

        public void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies)
        {
            var filteredTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(FilterType)
                .SelectMany(type => type.GetInterfaces())
                .Where(FilterInterfaceType);

            foreach (var type in filteredTypes) {
                var arguments = type.GetGenericArguments();

                var queryType = arguments.First();
                if (this._handlers.ContainsKey(queryType)) {
                    string errorMessage = string.Format(
                        "There are have duplicate IQueryHandler interface type for {0}.",
                        queryType.FullName);
                    throw new SystemException(errorMessage);
                }

                List<IHandler> queryHandlers = container.ResolveAll(type).OfType<IHandler>().ToList();
                if (queryHandlers.Count > 1) {
                    var errorMessage = string.Format(
                        "Found more than one handler for '{0}' with IQueryHandler.",
                        queryType.FullName);
                    throw new SystemException(errorMessage);
                }

                this._handlers[queryType] = queryHandlers.First();
                this._queryToResultMap[queryType] = arguments.Last();
            }

            foreach (Type queryType in Configuration.Current.QueryTypes.Values) {
                if (!this._handlers.ContainsKey(queryType)) {
                    var errorMessage = string.Format("The type('{0}') of handler is not found.", queryType.FullName);
                    throw new SystemException(errorMessage);
                }
            }
        }

        private static bool FilterInterfaceType(Type type)
        {
            if (!type.IsGenericType)
                return false;


            var genericType = type.GetGenericTypeDefinition();

            return genericType == typeof(IQueryHandler<,>);
        }

        private static bool FilterType(Type type)
        {
            if (!type.IsClass || type.IsAbstract)
                return false;

            return type.GetInterfaces().Any(FilterInterfaceType);
        }
        #endregion

        //private QueryResult CreateQueryResult(object metadata)
        //{
        //    return (QueryResult)createQueryResultMethod.MakeGenericMethod(metadata.GetType())
        //        .Invoke(null, new object[] { metadata });
        //}
    }
}
