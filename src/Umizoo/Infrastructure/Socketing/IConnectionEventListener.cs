using System;
using System.Net.Sockets;

namespace Umizoo.Infrastructure.Socketing
{
    public interface IConnectionEventListener
    {
        void OnConnectionAccepted(ITcpConnection connection);
        void OnConnectionEstablished(ITcpConnection connection);
        void OnConnectionFailed(SocketError socketError);
        void OnConnectionClosed(ITcpConnection connection, SocketError socketError);
    }
}
