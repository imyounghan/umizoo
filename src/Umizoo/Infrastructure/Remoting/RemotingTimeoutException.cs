using System;
using System.Net;

namespace Umizoo.Infrastructure.Remoting
{
    public class RemotingTimeoutException : TimeoutException
    {
        public RemotingTimeoutException(EndPoint serverEndPoint, RemotingRequest request, long timeoutMillis)
            : base(string.Format("Wait response from server[{0}] timeout, request:{1}, timeoutMillis:{2}ms", serverEndPoint, request, timeoutMillis))
        {
        }
    }
}
