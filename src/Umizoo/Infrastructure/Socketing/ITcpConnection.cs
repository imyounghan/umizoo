

namespace Umizoo.Infrastructure.Socketing
{
    using System.Net;

    public interface ITcpConnection
    {
        bool IsConnected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemotingEndPoint { get; }
        void QueueMessage(byte[] message);
        void Close();
    }
}
