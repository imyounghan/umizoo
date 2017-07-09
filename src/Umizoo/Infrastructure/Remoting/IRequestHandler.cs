namespace Umizoo.Infrastructure.Remoting
{
    public interface IRequestHandler
    {
        RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest);
    }
}
