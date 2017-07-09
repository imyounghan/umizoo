using System;
using System.Net.Sockets;
using Umizoo.Infrastructure.Socketing;

namespace Umizoo.Infrastructure.Remoting
{
    public class SocketRequestHandlerContext : IRequestHandlerContext
    {
        public ITcpConnection Connection { get; private set; }
        public Action<RemotingResponse> SendRemotingResponse { get; private set; }

        public SocketRequestHandlerContext(ITcpConnection connection, Action<byte[]> sendReplyAction)
        {
            Connection = connection;
            SendRemotingResponse = remotingResponse =>
            {
                sendReplyAction(RemotingUtil.BuildResponseMessage(remotingResponse));
            };
        }
    }
}
