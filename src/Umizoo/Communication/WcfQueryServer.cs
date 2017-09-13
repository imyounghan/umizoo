// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-11.

using System;
using System.Threading;
using System.Threading.Tasks;
using Umizoo.Configurations;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Async;
using Umizoo.Messaging;

namespace Umizoo.Communication
{
    public class WcfQueryServer : WcfServerChannel
    {
        private readonly IQueryService _queryService;
        private readonly ITextSerializer _serializer;

        public WcfQueryServer(IQueryService queryService, ITextSerializer serializer)
            : base(ProtocolCode.Query)
        {
            _queryService = queryService;
            _serializer = serializer;
        }

        public override IAsyncResult BeginExecute(Request request, AsyncCallback callback, object state)
        {
            var responseTask = new TaskCompletionSource<Response>();

            BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState)
            {
                if (_queryService.WaitingRequests > ConfigurationSettings.MaxRequests)
                {
                    responseTask.SetResult(Response.ServerTooBusy);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                Type type = null;

                if (!BasicTypes.QueryTypes.TryGetValue(request.Header.GetIfKeyNotFound("Type"), out type))
                {
                    responseTask.SetResult(Response.UnknownType);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                IQuery query;
                try
                {
                    query = (IQuery) _serializer.Deserialize(request.Body, type);
                }
                catch (Exception)
                {
                    responseTask.TrySetResult(Response.ParsingFailure);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                var timeout = request.Header.GetIfKeyNotFound("Timeout", "0").ChangeIfError(0);
                var result = _queryService.FetchAsync<object>(query, timeout).Result;
                var message = _serializer.Serialize(result);
                responseTask.TrySetResult(new Response(200, message));


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
    }
}