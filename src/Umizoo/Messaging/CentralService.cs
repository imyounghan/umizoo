

namespace Umizoo.Messaging
{
    using System;
    using System.Threading.Tasks;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;

    public class CentralService : ICommandService, IQueryService
    {
        private readonly IResultManager resultManger;
        private readonly IMessageBus<ICommand> commandBus;
        private readonly IMessageBus<IQuery> queryBus;

        public CentralService(IMessageBus<ICommand> commandBus, IMessageBus<IQuery> queryBus,
            IResultManager resultManger)
        {
            this.commandBus = commandBus;
            this.queryBus = queryBus;
            this.resultManger = resultManger;
        }

        #region ICommandService 成员

        public ICommandResult Execute(ICommand command, CommandReturnMode returnMode, int timeoutMs)
        {
            return this.ExecuteAsync(command, returnMode, timeoutMs).Result;
        }


        public Task<ICommandResult> ExecuteAsync(ICommand command, CommandReturnMode returnMode, int timeoutMs)
        {
            string traceId = ObjectId.GenerateNewStringId();
            var commandTask = resultManger.RegisterProcessingCommand(traceId, command, returnMode, timeoutMs);

            Task.Factory.StartNew(() => {
                try {
                    commandBus.Send(command, new TraceInfo(traceId, ConfigurationSettings.InnerAddress));
                    resultManger.SetCommandResult(traceId, CommandResult.Delivered);
                }
                catch (Exception ex) {
                    LogManager.Default.Error(ex);
                    resultManger.SetCommandResult(traceId, CommandResult.SentFailed);
                }
            });

            return commandTask;
        }
        #endregion

        protected int WaitingRequests
        {
            get { return resultManger.WaitingCommands + resultManger.WaitingQueries; }
        }

        #region IQueryService 成员

        protected Task<IQueryResult> FetchAsync(IQuery query, int timeoutMs)
        {
            string traceId = ObjectId.GenerateNewStringId();
            var queryTask = resultManger.RegisterProcessingQuery(traceId, query, timeoutMs);

            Task.Factory.StartNew(() => {
                try {
                    queryBus.Send(query, new TraceInfo(traceId, ConfigurationSettings.InnerAddress));
                }
                catch(Exception ex) {
                    LogManager.Default.Error(ex);
                    resultManger.SetQueryResult(traceId, QueryResult.SentFailed);
                }
            });

            return queryTask;
        }

        public T Fetch<T>(IQuery query, int timeoutMs)
        {
            return this.FetchAsync<T>(query,timeoutMs).Result;
        }

        public Task<T> FetchAsync<T>(IQuery query, int timeoutMs)
        {
            return this.FetchAsync(query, timeoutMs).ContinueWith(task => {
                if (task.Result.Status == HandleStatus.Success) {
                    return (T)task.Result.Data;
                }

                return default(T);
            });
        }

        #endregion
    }
}
