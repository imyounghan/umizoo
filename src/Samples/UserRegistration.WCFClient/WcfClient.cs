

namespace UserRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Umizoo;
    using Umizoo.Communication;
    using Umizoo.Infrastructure;
    using Umizoo.Messaging;

    public class WcfClient : DisposableObject, ICommandService, IQueryService
    {
        private readonly ITextSerializer _serializer;
        private readonly IClientChannelFactory _channelFactory;
        private readonly string commandClientConfigurationName;
        private readonly string queryClientConfigurationName;

        public int WaitingRequests => throw new NotImplementedException();

        public WcfClient(ITextSerializer serializer, IClientChannelFactory channelFactory)
        {
            _serializer = serializer;
            _channelFactory = channelFactory;

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
                Body = _serializer.Serialize(command)
            };
            request.Header = new Dictionary<string, string>()
                        {
                            { "Protocol", "1" },
                            { "Type", command.GetType().Name },
                            { "Model", ((int)returnType).ToString() },
                            { "Timeout", timeoutMs.ToString() },
                        };

            var channel = _channelFactory.GetChannel(commandClientConfigurationName, ProtocolCode.Command);
            if (channel == null)
            {
                throw new CommunicationException("Unable to connect to remote host.");
            }

            return
                Task.Factory.FromAsync<Request, Response>(channel.BeginExecute, channel.EndExecute, request, null)
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
            return _serializer.Deserialize<CommandResult>(response.Message);
        }


        public ICommandResult Execute(ICommand command, CommandReturnMode returnType, int timeoutMs)
        {
            return ExecuteAsync(command, returnType, timeoutMs).Result;
        }

        #endregion


        #region IQueryService 成员

        public Task<T> FetchAsync<T>(IQuery query, int timeoutMs)
        {
            var request = new Request() {
                Body = _serializer.Serialize(query)
            };
            request.Header = new Dictionary<string, string>()
                        {
                            { "Protocol", "2" },
                            { "Type", query.GetType().Name },
                            { "Timeout", timeoutMs.ToString() }
                        };

            var channel = _channelFactory.GetChannel(queryClientConfigurationName, ProtocolCode.Query);
            if (channel == null) {
                throw new CommunicationException("Unable to connect to remote host.");
            }

            return Task.Factory.FromAsync<Request, Response>(channel.BeginExecute, channel.EndExecute, request, null).ContinueWith<T>(this.Parse<T>);
        }

        public T Fetch<T>(IQuery query, int timeoutMs)
        {
            return FetchAsync<T>(query, timeoutMs).Result;
        }

        private T Parse<T>(Task<Response> task)
        {
            if (task.Exception == null) {
                var response = task.Result;
                if (response.Status == 200 && !string.IsNullOrEmpty(response.Message)) {
                    try {
                        var queryResult = _serializer.Deserialize<QueryResult<T>>(response.Message);

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
