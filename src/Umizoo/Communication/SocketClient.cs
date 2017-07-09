using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Remoting;
using Umizoo.Messaging;

namespace Umizoo.Communication
{
    /// <summary>
    /// 表示
    /// </summary>
    [Export("socket", typeof(ICommandService))]
    [Export("socket", typeof(IQueryService))]
    public class SocketClient : DisposableObject, ICommandService, IQueryService
    {
        private readonly string queryServiceAddress;
        private readonly string commandServiceAddress;

        private readonly ITextSerializer serializer;

        public SocketClient()
        {
            this.serializer = DefaultTextSerializer.Instance;

            this.queryServiceAddress = ConfigurationManager.AppSettings["umizoo_remotehost_query"].IfEmpty("127.0.0.1:9999");
            this.commandServiceAddress = ConfigurationManager.AppSettings["umizoo_remotehost_command"].IfEmpty("127.0.0.1:9999");
        }

        protected override void Dispose(bool disposing)
        {
        }
        
        #region ICommandService 成员

        public Task<ICommandResult> ExecuteAsync(ICommand command, CommandReturnMode returnType, int timeoutMs)
        {
            var request = new RemotingRequest() {
                Body = serializer.SerializeToBytes(command)
            };
            request.Code = (short)ProtocolCode.Command;
            request.Header = new Dictionary<string, string>()
                        {
                            { "Type", command.GetType().Name },
                            { "Model", ((int)returnType).ToString() },
                            { "Timeout", timeoutMs.ToString() }
                        };

            return Task.Factory.StartNew(() => SocketRemotingClientManager.Instance.GetRemotingClient(commandServiceAddress))
                .ContinueWith<ICommandResult>(task => {
                    var remotingClient = task.Result;

                    if (remotingClient == null || !remotingClient.IsConnected) {
                        var errorMessage = string.Format("Executing command failed as remotingClient is not connected, address({0})", commandServiceAddress);
                        if (remotingClient != null) {
                            LogManager.Default.Error(errorMessage);
                            remotingClient.Shutdown();
                        }

                        return new CommandResult(HandleStatus.Failed, errorMessage);
                    }

                    return remotingClient.InvokeAsync(request, timeoutMs + 1000).ContinueWith<ICommandResult>(Parse).Result;
                });          
        }

        private ICommandResult Parse(Task<RemotingResponse> task)
        {
            if (task.Exception != null) {
                return new CommandResult(HandleStatus.Failed, task.Exception.Message);
            }

            if (task.Result.RequestCode != 200) {
                var message = Encoding.UTF8.GetString(task.Result.ResponseBody);
                return new CommandResult(HandleStatus.Failed, message);
            }
            
            return serializer.Deserialize<CommandResult>(task.Result.ResponseBody);
        }


        public ICommandResult Execute(ICommand command, CommandReturnMode returnType, int timeoutMs)
        {
            return this.ExecuteAsync(command, returnType, timeoutMs).Result;

        }

        #endregion


        #region
        public Task<T> FetchAsync<T>(IQuery query, int timeoutMs)
        {
            var request = new RemotingRequest() {
                Body = serializer.SerializeToBytes(query)
            };
            request.Code = (short)ProtocolCode.Query;
            request.Header = new Dictionary<string, string>()
                        {
                            { "Type", query.GetType().Name },
                            { "Timeout", timeoutMs.ToString() }
                        };

            return Task.Factory.StartNew(() => SocketRemotingClientManager.Instance.GetRemotingClient(queryServiceAddress))
                .ContinueWith<T>(task => {
                    var remotingClient = task.Result;
                    if (remotingClient == null || !remotingClient.IsConnected) {
                        var errorMessage = string.Format("Executing query failed as remotingClient is not connected, address({0})", queryServiceAddress);
                        if (remotingClient != null) {
                            LogManager.Default.Error(errorMessage);
                            remotingClient.Shutdown();
                        }

                        return default(T);
                    }

                    return remotingClient.InvokeAsync(request, timeoutMs + 1000).ContinueWith<T>(this.Parse<T>).Result;
                });
        }

        public T Fetch<T>(IQuery query, int timeoutMs)
        {
            return this.FetchAsync<T>(query, timeoutMs).Result;
        }

        private T Parse<T>(Task<RemotingResponse> task)
        {
            if(task.Exception != null || task.IsFaulted) {
                LogManager.Default.Error(task.Exception, "Parse the result encountered an error.");

                return default(T);
            }

            var response = task.Result;
            if (response.ResponseCode != 200) {
                LogManager.Default.ErrorFormat("Server query failed. result:{0}.", Encoding.UTF8.GetString(response.ResponseBody));

                return default(T);
            }

            if(response.ResponseBody == null || response.ResponseBody.Length == 0) {
                LogManager.Default.Error("Server queried as empty results.");
                return default(T);
            }

            try {
                var queryResult = serializer.Deserialize<QueryResult<T>>(response.ResponseBody);
                return queryResult.Data;
            }
            catch (Exception ex) {
                LogManager.Default.Error(ex, "Failed to deserialize type of '{0}' from ({1}).", typeof(T).FullName,
                    Encoding.UTF8.GetString(response.ResponseBody));
            }

            return default(T);
        }

        #endregion        

    }
}
