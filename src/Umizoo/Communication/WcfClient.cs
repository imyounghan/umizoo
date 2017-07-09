

namespace UserRegistration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Configuration;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Umizoo;
    using Umizoo.Communication;
    using Umizoo.Infrastructure;
    using Umizoo.Messaging;

    [Export(typeof(ICommandService))]
    [Export(typeof(IQueryService))]
    public class WcfClient : DisposableObject, ICommandService, IQueryService
    {
        private readonly ITextSerializer serializer;
        private readonly string commandClientConfigurationName;
        private readonly string queryClientConfigurationName;

        public WcfClient()
        {
            this.serializer = DefaultTextSerializer.Instance;

            this.commandClientConfigurationName = ConfigurationManager.AppSettings["umizoo.wcfclient_command"].IfEmpty("wcfClient");
            this.queryClientConfigurationName = ConfigurationManager.AppSettings["umizoo.wcfclient_query"].IfEmpty("wcfClient");
        }

        protected override void Dispose(bool disposing)
        {
        }

        #region ICommandService 成员

        public Task<ICommandResult> ExecuteAsync(ICommand command, CommandReturnMode returnType, int timeoutMs)
        {
            var request = new Request() {
                Body = serializer.Serialize(command)
            };
            request.Header = new Dictionary<string, string>()
                        {
                            { "Protocol", "1" },
                            { "Type", command.GetType().Name },
                            { "Model", ((int)returnType).ToString() },
                            { "Timeout", timeoutMs.ToString() },
                        };

            var channelFactory = WcfRemotingClientManager.Instance.GetChannelFactory(commandClientConfigurationName);
            if (channelFactory == null)
            {
                throw new CommunicationException("Unable to connect to remote host.");
            }

            var proxy = channelFactory.CreateChannel();

            return
                Task.Factory.FromAsync<Request, Response>(proxy.BeginExecute, proxy.EndExecute, request, null)
                    .ContinueWith<ICommandResult>(this.Parse);
        }

        private ICommandResult Parse(Task<Response> task)
        {
            if (task.Exception != null)
            {
                return new CommandResult(HandleStatus.Failed, task.Exception.Message);
            }

            if (task.Result.Status != 200) {
                return new CommandResult(HandleStatus.Failed, task.Result.Message);
            }

            var response = task.Result;
            return serializer.Deserialize<CommandResult>(response.Message);
        }


        public ICommandResult Execute(ICommand command, CommandReturnMode returnType, int timeoutMs)
        {
            return this.ExecuteAsync(command, returnType, timeoutMs).Result;
        }

        #endregion


        #region IQueryService 成员

        public Task<T> FetchAsync<T>(IQuery query, int timeoutMs)
        {
            var request = new Request() {
                Body = serializer.Serialize(query)
            };
            request.Header = new Dictionary<string, string>()
                        {
                            { "Protocol", "2" },
                            { "Type", query.GetType().Name },
                            { "Timeout", timeoutMs.ToString() }
                        };

            var proxy = WcfRemotingClientManager.Instance.GetChannelFactory(queryClientConfigurationName).CreateChannel();

            return Task.Factory.FromAsync<Request, Response>(proxy.BeginExecute, proxy.EndExecute, request, null).ContinueWith<T>(this.Parse<T>);
        }

        public T Fetch<T>(IQuery query, int timeoutMs)
        {
            return this.FetchAsync<T>(query, timeoutMs).Result;
        }

        private T Parse<T>(Task<Response> task)
        {
            if (task.Exception == null) {
                var response = task.Result;
                if (response.Status == 200 && !string.IsNullOrEmpty(response.Message)) {
                    try {
                        var queryResult = serializer.Deserialize<QueryResult<T>>(response.Message);

                        return queryResult.Data;
                    }
                    catch (Exception) {
                    }
                }
            }

            return default(T);
        }

        #endregion        
    }
}
