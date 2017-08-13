// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-10.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Umizoo.Infrastructure.Async;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure;
using Umizoo.Messaging;

namespace Umizoo.Communication
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [DataContractFormat(Style = OperationFormatStyle.Rpc)]
    public class WcfReplyServer : WcfServerChannel, IInitializer
    {
        private readonly IMessageBus<IResult> _resultBus;
        private readonly Dictionary<string, Type> _resultTypeMap;
        private readonly ITextSerializer _serializer;
        
        public WcfReplyServer(IMessageBus<IResult> resultBus, ITextSerializer serializer)
            : base(ProtocolCode.Reply)
        {
            _resultBus = resultBus;
            _serializer = serializer;
            _resultTypeMap = new Dictionary<string, Type>()
            {
                { "CommandResult", typeof(CommandResult) },
                { "QueryResult", typeof(QueryResult) }
            };
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            types.Where(type => type.IsClass && !type.IsAbstract && typeof(IResult).IsAssignableFrom(type)).ForEach(
                type => { _resultTypeMap[type.Name] = type; });
        }

        public override IAsyncResult BeginExecute(Request request, AsyncCallback callback, object state)
        {
            var responseTask = new TaskCompletionSource<Response>();

            BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState)
            {
                var typeName = request.Header.GetIfKeyNotFound("Type");
                Type type;
                if (!_resultTypeMap.TryGetValue(typeName, out type))
                {
                    responseTask.SetResult(Response.UnknownType);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                var traceId = request.Header.GetIfKeyNotFound("TraceId");
                if (string.IsNullOrEmpty(traceId))
                {
                    var errorMessage = string.Format("Receive a empty traceId Reply Type({0}).", typeName);
                    responseTask.SetResult(new Response(500, errorMessage));
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                IResult result;
                try
                {
                    result = (IResult)_serializer.Deserialize(request.Body, type);
                }
                catch (Exception)
                {
                    responseTask.TrySetResult(Response.ParsingFailure);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                _resultBus.Send(result, traceId);
                return CreateInnerAsyncResult(asyncCallback, asyncState);
            };

            EndInvokeDelegate<Response> endDelegate =
                delegate { return responseTask.Task.Result; };

            return WrappedAsyncResult<Response>.Begin(
                callback,
                state,
                beginDelegate,
                endDelegate,
                null,
                Timeout.Infinite);
        }

        public override Response EndExecute(IAsyncResult asyncResult)
        {
            return Response.Success;
        }
    }
}