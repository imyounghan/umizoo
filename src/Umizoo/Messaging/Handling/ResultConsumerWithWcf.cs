

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Umizoo.Communication;
    using Umizoo.Infrastructure;

    public class ResultConsumerWithWcf : ReplyResultConsumer
    {
        private readonly ITextSerializer serializer;

        public ResultConsumerWithWcf(ITextSerializer serializer, IMessageReceiver<Envelope<IResult>> resultReceiver)
            : base(resultReceiver)
        {
            this.serializer = serializer;
        }

        protected override void Dispose(bool disposing)
        {
        }
        protected override bool SendReply(IResult result, TraceInfo traceInfo)
        {
            var request = new Request() {
                Body = serializer.Serialize(result)
            };
            request.Header = new Dictionary<string, string>()
                        {
                            { "Type", result.GetType().Name },
                            { "TraceId", traceInfo.Id },
                        };

            var channelFactory = WcfRemotingClientManager.Instance.GetChannelFactory(traceInfo.Address);

            if (channelFactory == null) {
                LogManager.Default.ErrorFormat("Send reply result failed as GetOrCreate ChannelFactory failed, address({0}).", traceInfo.Address);
                return false;
            }


            IContract proxy;
            try
            {
                proxy = channelFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex, "Send reply result failed as Create Channel failed, address({0}).", traceInfo.Address);
                WcfRemotingClientManager.Instance.CloseChannelFactory(traceInfo.Address);

                return false;
            }


            //Task.Factory.FromAsync<Request, Response>(proxy.BeginExecute, proxy.EndExecute, request, null)
            //    .ContinueWith(
            //        task => {
            //            if(LogManager.Default.IsDebugEnabled) {
            //                LogManager.Default.DebugFormat("Send {0}({1}) success, traceId:{2},traceAddress:{3}.", 
            //                    request.Header["Type"], request.Body,
            //                    traceInfo.Id, traceInfo.Address);
            //            }
            //        });

            proxy.BeginExecute(request, null, null);

            return true;
        }        
    }
}
