using System;
using Umizoo.Infrastructure.Socketing;

namespace Umizoo.Infrastructure.Remoting
{
    public interface IRequestHandlerContext
    {
        ITcpConnection Connection { get; }
        Action<RemotingResponse> SendRemotingResponse { get; }
    }
}
