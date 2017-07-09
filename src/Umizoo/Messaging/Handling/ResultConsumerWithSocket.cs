using System.Collections.Generic;
using Umizoo.Communication;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Remoting;

namespace Umizoo.Messaging.Handling
{
    public class ResultConsumerWithSocket : ReplyResultConsumer
    {
        private readonly ITextSerializer serializer;

        public ResultConsumerWithSocket(ITextSerializer serializer, IMessageReceiver<Envelope<IResult>> resultReceiver)
            : base(resultReceiver)
        {
            this.serializer = serializer;
        }

        protected override void Dispose(bool disposing)
        {
            
        }

        protected override bool SendReply(IResult result, TraceInfo traceInfo)
        {
            var request = new RemotingRequest() {
                Body = serializer.SerializeToBytes(result)
            };
            request.Code = (short)ProtocolCode.Notify;
            request.Header = new Dictionary<string, string>()
                        {
                            { "Type", result.GetType().Name },
                            { "TraceId", traceInfo.Id },
                        };

            var remotingClient = SocketRemotingClientManager.Instance.GetRemotingClient(traceInfo.Address);

            if (remotingClient == null) {
                var errorMessage = string.Format("Send reply result failed as remotingClient is not connected, address({0}).", traceInfo.Address);
                LogManager.Default.Error(errorMessage);
                return false;
            }

            if (!remotingClient.IsConnected) {
                var errorMessage = string.Format("Send reply result failed as remotingClient is disconnected, address({0}).", traceInfo.Address);
                LogManager.Default.Error(errorMessage);
                SocketRemotingClientManager.Instance.CloseRemotingClient(traceInfo.Address);

                return false;
            }

            remotingClient.InvokeOneway(request);

            return true;
        }
    }
}
