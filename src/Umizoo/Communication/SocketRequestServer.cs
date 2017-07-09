

namespace Umizoo.Communication
{
    using System;
    using System.Net;
    using System.Text;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Remoting;
    using Umizoo.Messaging;

    public class SocketRequestServer : CentralService, IRequestHandler, IProcessor
    {
        private readonly SocketRemotingServer _remotingServer;

        private readonly ITextSerializer serializer;

        private readonly object lockObject;

        public SocketRequestServer(IMessageBus<ICommand> commandBus,
            IMessageBus<IQuery> queryBus,
            ITextSerializer serializer,
            IResultManager resultManger)
            : base(commandBus, queryBus, resultManger)
        {
            this.serializer = serializer;
            this.lockObject = new object();

            var items = ConfigurationSettings.InnerAddress.Split(':');
            var bindingOutAddress = new IPEndPoint(IPAddress.Parse(items[0]), items[1].ChangeIfError(9999));
            _remotingServer = new SocketRemotingServer(ConfigurationSettings.ServiceName, bindingOutAddress);
        }


        #region IProcessor 成员
        private bool started;

        private void Start()
        {
            _remotingServer.Start();
            _remotingServer.RegisterRequestHandler((int)ProtocolCode.Command, this);
            _remotingServer.RegisterRequestHandler((int)ProtocolCode.Query, this);
        }

        private void Stop()
        {
            _remotingServer.Shutdown();
        }

        void IProcessor.Start()
        {
            lock (this.lockObject) {
                if (!this.started) {
                    this.Start();
                    this.started = true;
                }
            }
        }

        void IProcessor.Stop()
        {
            lock (this.lockObject) {
                if (this.started) {
                    this.Stop();
                    this.started = false;
                }
            }
        }

        #endregion


        #region IRequestHandler 成员

        public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest)
        {
            if (this.WaitingRequests > ConfigurationSettings.MaxRequests) {
                return new RemotingResponse(remotingRequest) {
                    ResponseCode = 500,
                    ResponseBody = Encoding.UTF8.GetBytes("Server Too Busy."),
                    ResponseTime = DateTime.UtcNow
                };
            }

            string typeName = remotingRequest.Header.GetIfKeyNotFound("Type", string.Empty);
            Type type = null;
            bool typeConfirmed = false;
            switch ((ProtocolCode)remotingRequest.Code) {
                case ProtocolCode.Command:
                    typeConfirmed = Configuration.Current.CommandTypes.TryGetValue(typeName, out type);
                    break;
                case ProtocolCode.Query:
                    typeConfirmed = Configuration.Current.QueryTypes.TryGetValue(typeName, out type);
                    break;
            }

            if (!typeConfirmed) {
                return new RemotingResponse(remotingRequest) {
                    ResponseCode = 404,
                    ResponseBody = Encoding.UTF8.GetBytes("Unkonw Type."),
                    ResponseTime = DateTime.UtcNow
                };
            }

            object graph;
            try {
                graph = serializer.Deserialize(remotingRequest.Body, type);
            }
            catch (Exception) {
                return new RemotingResponse(remotingRequest) {
                    ResponseCode = 500,
                    ResponseBody = Encoding.UTF8.GetBytes("Serialization failure."),
                    ResponseTime = DateTime.UtcNow
                };
            }

            var timeout = remotingRequest.Header.GetIfKeyNotFound("Timeout", "0").ChangeIfError(0);
            switch ((ProtocolCode)remotingRequest.Code) {
                case ProtocolCode.Command:
                    var returnMode = (CommandReturnMode)remotingRequest.Header.GetIfKeyNotFound("Mode", "1").ChangeIfError(1);
                    var commandResult = this.ExecuteAsync((ICommand)graph, returnMode, timeout).Result;
                    return new RemotingResponse(remotingRequest) {
                        ResponseCode = 200,
                        ResponseBody = serializer.SerializeToBytes(commandResult),
                        ResponseTime = DateTime.UtcNow
                    };
                case ProtocolCode.Query:
                    var queryResult = this.FetchAsync((IQuery)graph, timeout).Result;
                    return new RemotingResponse(remotingRequest) {
                        ResponseCode = 200,
                        ResponseBody = serializer.SerializeToBytes(queryResult),
                        ResponseTime = DateTime.UtcNow
                    };
            }

            return null;
        }

        #endregion
    }
}
