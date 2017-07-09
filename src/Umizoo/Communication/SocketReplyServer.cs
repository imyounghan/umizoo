

namespace Umizoo.Communication
{
    using System;
    using System.Net;
    using System.Text;
    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Remoting;
    using Umizoo.Messaging;

    public class SocketReplyServer : IRequestHandler, IProcessor
    {
        private readonly SocketRemotingServer _remotingServer;

        private readonly ITextSerializer serializer;
        private readonly IMessageBus<IResult> replyBus;

        private readonly object lockObject;

        public SocketReplyServer(IMessageBus<IResult> replyBus,
            ITextSerializer serializer)
        {
            this.serializer = serializer;
            this.replyBus = replyBus;
            this.lockObject = new object();

            var items = ConfigurationSettings.InnerAddress.Split(':');
            var bindingInnerAddress = new IPEndPoint(IPAddress.Parse(items[0]), items[1].ChangeIfError(9999));
            _remotingServer = new SocketRemotingServer(ConfigurationSettings.ServiceName, bindingInnerAddress);
        }


        #region IProcessor 成员
        private bool started;

        private void Start()
        {
            _remotingServer.Start();
            _remotingServer.RegisterRequestHandler((int)ProtocolCode.Notify, this);
        }

        private void Stop()
        {
            _remotingServer.Shutdown();
        }

        void IProcessor.Start()
        {
            lock(this.lockObject) {
                if(!this.started) {
                    this.Start();
                    this.started = true;
                }
            }
        }

        void IProcessor.Stop()
        {
            lock(this.lockObject) {
                if(this.started) {
                    this.Stop();
                    this.started = false;
                }
            }
        }

        #endregion
        

        #region IRequestHandler 成员

        public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest)
        {
            string typeName = remotingRequest.Header.GetIfKeyNotFound("Type");
            Type type;
            if(string.IsNullOrEmpty(typeName) || !Configuration.Current.ResultTypes.TryGetValue(typeName, out type)) {
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

            var traceId = remotingRequest.Header.GetIfKeyNotFound("TraceId");
            if (string.IsNullOrEmpty(traceId))
            {
                return new RemotingResponse(remotingRequest) {
                    ResponseCode = 500,
                    ResponseBody = Encoding.UTF8.GetBytes("TraceId is empty."),
                    ResponseTime = DateTime.UtcNow
                };
            }

            replyBus.Send((IResult)graph, traceId);
            return new RemotingResponse(remotingRequest) {
                ResponseCode = 200,
                ResponseTime = DateTime.UtcNow
            };
        }

        #endregion
    }
}
