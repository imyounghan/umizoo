// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Threading.Tasks;

using Umizoo.Configurations;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging
{
    public class QueryService : IQueryService
    {
        private readonly IResultManager _resultManger;
        private readonly IMessageBus<IQuery> _queryBus;

        public int WaitingRequests
        {
            get { return _resultManger.WaitingQueries; }
        }

        public QueryService(IMessageBus<IQuery> queryBus, IResultManager resultManger)
        {
            _queryBus = queryBus;
            _resultManger = resultManger;
        }


        public Task<IQueryResult> FetchAsync(IQuery query, int timeoutMs)
        {
            var traceId = ObjectId.GenerateNewId().ToString();
            var queryTask = _resultManger.RegisterProcessingQuery(traceId, query, timeoutMs);

            Task.Factory.StartNew(() => {
                try {
                    _queryBus.Send(query, new TraceInfo(traceId, ConfigurationSettings.InnerAddress));
                }
                catch (Exception ex) {
                    LogManager.Default.Error(ex);
                    _resultManger.SetQueryResult(traceId, QueryResult.SentFailed);
                }
            });

            return queryTask;
        }

        public T Fetch<T>(IQuery query, int timeoutMs)
        {
            return FetchAsync<T>(query, timeoutMs).Result;
        }

        public Task<T> FetchAsync<T>(IQuery query, int timeoutMs)
        {
            return FetchAsync(query, timeoutMs).ContinueWith(task => {
                if (task.Result.Status == HandleStatus.Success) {
                    return (T)task.Result.Data;
                }

                return default(T);
            });
        }
    }
}