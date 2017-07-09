namespace Umizoo.Infrastructure.Remoting
{
    public interface IResponseHandler
    {
        void HandleResponse(RemotingResponse remotingResponse);
    }
}
