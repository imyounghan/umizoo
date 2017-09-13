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
using Umizoo.Configurations;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Composition.Interception;

namespace Umizoo.Messaging.Handling
{
    public class QueryConsumer : Consumer<IQuery>, IInitializer
    {
        private readonly Dictionary<Type, HandlerDescriptor> _handlerDescriptors;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly SemaphoreSlim _semaphore;
        private readonly HandlerPipelineManager _pipelineManager;


        public QueryConsumer(IMessageReceiver<Envelope<IQuery>> receiver,
            IMessageBus<IResult> resultBus)
            : base(receiver, ProcessingFlags.Query)
        {
            _handlerDescriptors = new Dictionary<Type, HandlerDescriptor>();
            _pipelineManager = new HandlerPipelineManager();
            _resultBus = resultBus;

            var threadCount = ConfigurationSettings.ParallelQueryThead;
            if (threadCount > 1) _semaphore = new SemaphoreSlim(threadCount, threadCount);
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            var contactTypeMap = new Dictionary<Type, Type>();
            types.Where(type => type.IsClass && !type.IsAbstract)
                .ForEach(type => {
                    type.GetInterfaces().Where(interfaceType => interfaceType.IsGenericType &&
                                   interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                        .ForEach(interfaceType => {
                            var queryType = interfaceType.GetGenericArguments().First();
                            if (contactTypeMap.ContainsKey(queryType)) {
                                var errorMessage = string.Format(
                                    "There are have duplicate IQueryHandler<> interface type for {0}.",
                                    queryType.FullName);
                                throw new SystemException(errorMessage);
                            }

                            contactTypeMap[queryType] = interfaceType;
                        });
                });

            BasicTypes.QueryTypes.Values.ForEach(queryType => {
                Type contactType;
                if (!contactTypeMap.TryGetValue(queryType, out contactType)) {
                    var errorMessage = string.Format("The query handler of this type('{0}') is not found.",
                        queryType.FullName);
                    throw new SystemException(errorMessage);
                }

                var handlers = container.ResolveAll(contactType).ToList();
                switch (handlers.Count) {
                    case 0:
                        var errorMessage = string.Format("The query handler for {0} is not found.",
                            contactType.GetFriendlyTypeName());
                        throw new SystemException(errorMessage);
                    case 1:
                        var handlerDescriptor = new HandlerDescriptor(handlers[0], handlerType => handlerType.GetMethod("Handle", new[] { queryType }), HandlerStyle.Special);
                        _handlerDescriptors[queryType] = handlerDescriptor;
                        var callHandlers = HandlerAttributeHelper.GetHandlersFor(handlerDescriptor.Method, container);
                        _pipelineManager.InitializePipeline(handlerDescriptor.Method, callHandlers);
                        break;
                    default:
                        errorMessage = string.Format(
                            "Found more than one handler for '{0}' with IQueryHandler<>.",
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
            var handlerDescriptor = _handlerDescriptors[envelope.Body.GetType()];


            if (ConfigurationSettings.ParallelQueryThead > 1)
            {
                _semaphore.Wait();
                Task.Factory.StartNew(() =>
                {
                    ExecutingQuery(handlerDescriptor, envelope);
                    _semaphore.Release();
                });
                return;
            }

            ExecutingQuery(handlerDescriptor, envelope);
        }

        private void ExecutingQuery(HandlerDescriptor handlerDescriptor, Envelope<IQuery> envelope)
        {
            var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];

            var methodInvocation = new MethodInvocation(handlerDescriptor.Target, handlerDescriptor.Method, envelope.Body);
            methodInvocation.InvocationContext[StandardMetadata.TraceInfo] = traceInfo;
            methodInvocation.InvocationContext[StandardMetadata.MessageId] = envelope.MessageId;

            try {
                var methodReturn = _pipelineManager.GetPipeline(handlerDescriptor.Method).Invoke(methodInvocation,
                    delegate (IMethodInvocation input, GetNextHandlerDelegate getNext) {
                        var result = handlerDescriptor.Invode(input.Arguments.Cast<object>().ToArray());
                        return input.CreateMethodReturn(result);
                    });
                NotifyResult(traceInfo, methodReturn.Exception ?? methodReturn.ReturnValue);
            }
            catch (Exception ex) {
                NotifyResult(traceInfo, ex);
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
    }
}