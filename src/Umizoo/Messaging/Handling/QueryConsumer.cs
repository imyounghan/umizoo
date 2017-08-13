// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Filtering;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public class QueryConsumer : Consumer<IQuery>, IInitializer
    {
        private readonly Dictionary<Type, IHandler> _handlers;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly SemaphoreSlim _semaphore;


        public QueryConsumer(IMessageReceiver<Envelope<IQuery>> receiver,
            IMessageBus<IResult> resultBus)
            : base(receiver, ProcessingFlags.Query)
        {
            _handlers = new Dictionary<Type, IHandler>();
            _resultBus = resultBus;

            var threadCount = ConfigurationSettings.ParallelQueryThead;
            if (threadCount > 1) _semaphore = new SemaphoreSlim(threadCount, threadCount);
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            var contactTypeMap = new Dictionary<Type, Type>();
            types.Where(type => type.IsClass && !type.IsAbstract && typeof(IQueryHandler).IsAssignableFrom(type))
                .ForEach(type => {
                    type.GetInterfaces().Where(interfaceType => interfaceType.IsGenericType &&
                                   interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                        .ForEach(interfaceType => {
                            var queryType = interfaceType.GetGenericArguments().First();
                            if (contactTypeMap.ContainsKey(queryType)) {
                                var errorMessage = string.Format(
                                    "There are have duplicate IQueryHandler interface type for {0}.",
                                    queryType.FullName);
                                throw new SystemException(errorMessage);
                            }

                            contactTypeMap[queryType] = interfaceType;
                        });
                });

            //types.Where(type => type.IsInterface && type.IsGenericType &&
            //                       type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            //    .ForEach(contractType =>
            //    {
            //        var queryType = contractType.GetGenericArguments().First();
            //        if (contactTypeMap.ContainsKey(queryType))
            //        {
            //            var errorMessage = string.Format(
            //                "There are have duplicate IQueryHandler interface type for {0}.",
            //                queryType.FullName);
            //            throw new SystemException(errorMessage);
            //        }

            //        contactTypeMap[queryType] = contractType;
            //    });

            types.Where(type => type.IsClass && !type.IsAbstract && typeof(IQuery).IsAssignableFrom(type))
                .ForEach(queryType =>
                {
                    Type contactType;
                    if (!contactTypeMap.TryGetValue(queryType, out contactType))
                    {
                        var errorMessage = string.Format("The query handler of this type('{0}') is not found.",
                            queryType.FullName);
                        throw new SystemException(errorMessage);
                    }

                    var queryHandlers = container.ResolveAll(contactType).OfType<IHandler>().ToList();
                    switch (queryHandlers.Count)
                    {
                        case 0:
                            var errorMessage = string.Format("The query handler for {0} is not found.",
                                contactType.GetFriendlyTypeName());
                            throw new SystemException(errorMessage);
                        case 1:
                            _handlers[queryType] = queryHandlers[0];
                            break;
                        default:
                            errorMessage = string.Format(
                                "Found more than one handler for '{0}' with IQueryHandler.",
                                queryType.FullName);
                            throw new SystemException(errorMessage);
                    }
                });
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override void OnMessageReceived(Envelope<IQuery> envelope)
        {
            var queryType = envelope.Body.GetType();

            var traceInfo = (TraceInfo) envelope.Items[StandardMetadata.TraceInfo];

            IHandler handler;
            if (!_handlers.TryGetValue(queryType, out handler))
            {
                var errorMessage = string.Format("The handler of this type('{0}') is not found.", queryType.FullName);
                if (LogManager.Default.IsDebugEnabled) LogManager.Default.Debug(errorMessage);
                NotifyResult(traceInfo, new QueryResult(HandleStatus.Failed, errorMessage));
                return;
            }


            if (ConfigurationSettings.ParallelQueryThead > 1)
            {
                _semaphore.Wait();
                Task.Factory.StartNew(() =>
                {
                    ExecutingQuery(handler, envelope.Body, traceInfo);
                    _semaphore.Release();
                });
                return;
            }

            ExecutingQuery(handler, envelope.Body, traceInfo);
        }

        private void ExecutingQuery(IHandler handler, IQuery query, TraceInfo traceInfo)
        {
            if (!ConfigurationSettings.QueryFilterEnabled)
            {
                try
                {
                    var result = Execute(handler, query);
                    NotifyResult(traceInfo, result);
                }
                catch (Exception ex)
                {
                    NotifyResult(traceInfo, ex);
                }

                return;
            }


            var handlerContext = new HandlerContext(query, handler);
            handlerContext.InvocationContext[StandardMetadata.MessageId] = traceInfo.Id;

            var filters = FilterProviders.Providers.GetFilters(handlerContext);
            var filterInfo = new FilterInfo(filters);

            try
            {
                var postContext = InvokeHandlerMethodWithFilters(
                    handlerContext,
                    filterInfo.ActionFilters,
                    query);
                if (!postContext.ExceptionHandled)
                    NotifyResult(traceInfo, postContext.ReturnValue ?? postContext.Exception);
                else NotifyResult(traceInfo, postContext.ReturnValue);
            }
            catch (Exception ex)
            {
                var exceptionContext = InvokeExceptionFilters(
                    handlerContext,
                    filterInfo.ExceptionFilters,
                    ex);
                if (!exceptionContext.ExceptionHandled)
                    NotifyResult(traceInfo, exceptionContext.ReturnValue ?? exceptionContext.Exception);
                else NotifyResult(traceInfo, exceptionContext.ReturnValue);
            }
        }

        private void NotifyResult(TraceInfo traceInfo, object result)
        {
            var ex = result as Exception;

            QueryResult queryResult;

            // 如果出现了系统异常则将异常转换成返回结果发送
            if (ex != null)
            {
                queryResult = new QueryResult(HandleStatus.Failed, ex.Message);
                _resultBus.Send(queryResult, traceInfo);
                return;
            }

            if (result == null || result == DBNull.Value)
            {
                queryResult = new QueryResult(HandleStatus.Nothing);
                _resultBus.Send(queryResult, traceInfo);
                return;
            }

            var enumerable = result as IEnumerable;
            if (enumerable != null && !enumerable.GetEnumerator().MoveNext())
                queryResult = new QueryResult(HandleStatus.Nothing);
            else queryResult = new QueryResult(result);
            _resultBus.Send(queryResult, traceInfo);
        }

        private static ExceptionContext InvokeExceptionFilters(
            HandlerContext queryHandlerContext,
            IList<IExceptionFilter> filters,
            Exception exception)
        {
            var context = new ExceptionContext(queryHandlerContext, exception);
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
                return new ActionExecutedContext(preContext, true, null) {ReturnValue = preContext.ReturnValue};

            var wasError = false;
            ActionExecutedContext postContext;

            try
            {
                postContext = continuation();
            }
            catch (Exception ex)
            {
                wasError = true;
                postContext = new ActionExecutedContext(preContext, false, ex);
                filter.OnActionExecuted(postContext);
                if (!postContext.ExceptionHandled) throw;
            }

            if (!wasError) filter.OnActionExecuted(postContext);

            return postContext;
        }

        private ActionExecutedContext InvokeHandlerMethodWithFilters(
            HandlerContext handlerContext,
            IList<IActionFilter> filters,
            IQuery query)
        {
            var preContext = new ActionExecutingContext(handlerContext);

            Func<ActionExecutedContext> continuation = () => new ActionExecutedContext(handlerContext, false, null)
            {
                ReturnValue = Execute(handlerContext.Handler, query)
            };

            var thunk = filters.Reverse().Aggregate(continuation,
                (next, filter) => () => InvokeHandlerMethodFilter(filter, preContext, next));
            return thunk();
        }

        private object Execute(IHandler handler, IQuery query)
        {
            var retryTimes = ConfigurationSettings.HandleRetrytimes;
            var retryInterval = ConfigurationSettings.HandleRetryInterval;

            var count = 0;
            object result = null;
            while (count++ < retryTimes)
                try
                {
                    result = ((dynamic) handler).Handle((dynamic) query);
                    break;
                }
                catch (Exception ex)
                {
                    if (count == retryTimes)
                    {
                        LogManager.Default.Error(
                            ex,
                            "Exception raised when handling '{0}({1})' on '{2}', the retry count has been reached.",
                            query.GetType().FullName,
                            TextSerializer.Instance.Serialize(query),
                            handler.GetType().FullName);
                        throw ex;
                    }

                    Thread.Sleep(retryInterval);
                }

            if (LogManager.Default.IsDebugEnabled)
                LogManager.Default.DebugFormat("Handle '{0}({1})' on '{2}' successfully.",
                    query.GetType().FullName,
                    TextSerializer.Instance.Serialize(query),
                    handler.GetType().FullName);

            return result;
        }
    }
}