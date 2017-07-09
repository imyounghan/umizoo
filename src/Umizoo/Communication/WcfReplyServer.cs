

namespace Umizoo.Communication
{
    using System;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Async;
    using Umizoo.Messaging;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [DataContractFormat(Style = OperationFormatStyle.Rpc)]
    public class WcfReplyServer : Processor, IContract
    {
        private readonly IMessageBus<IResult> resultBus;
        private readonly ITextSerializer serializer;

        public WcfReplyServer(IMessageBus<IResult> resultBus, ITextSerializer serializer)
        {
            this.resultBus = resultBus;
            this.serializer = serializer;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
            }
        }

        private ServiceHost wcfHost;

        protected override void Start()
        {

            wcfHost = new ServiceHost(this);
            wcfHost.Opened += (sender, e) => Console.WriteLine("WCF Reply Service Started.");
            wcfHost.Open();
        }

        protected override void Stop()
        {
            using(wcfHost) {
                wcfHost.Close();
                wcfHost = null;
            }
        }

        #region IContract 成员
        
        private IAsyncResult CreateInnerAsyncResult(AsyncCallback asyncCallback, object asyncState)
        {
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(asyncState);
            asyncResult.MarkCompleted(true /* completedSynchronously */, asyncCallback);
            return asyncResult;
        }

        public IAsyncResult BeginExecute(Request request, AsyncCallback callback, object state)
        {
            TaskCompletionSource<Response> responseTask = new TaskCompletionSource<Response>();

            BeginInvokeDelegate beginDelegate = delegate (AsyncCallback asyncCallback, object asyncState) {

                string typeName = request.Header.GetIfKeyNotFound("Type");
                Type type;
                if (!Configuration.Current.ResultTypes.TryGetValue(typeName, out type)) {
                    responseTask.SetResult(Response.UnknownType);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                var traceId = request.Header.GetIfKeyNotFound("TraceId");
                if (string.IsNullOrEmpty(traceId)) {
                    var errorMessage = string.Format("Receive a empty traceId Reply Type({0}).", typeName);
                    responseTask.SetResult(new Response(500, errorMessage));
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                IResult result;
                try {
                    result = (IResult)serializer.Deserialize(request.Body, type);
                }
                catch (Exception) {
                    responseTask.TrySetResult(Response.ParsingFailure);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                resultBus.Send(result, traceId);
                return CreateInnerAsyncResult(asyncCallback, asyncState);
            };

            EndInvokeDelegate<Response> endDelegate =
                delegate (IAsyncResult asyncResult) { return responseTask.Task.Result; };

            return WrappedAsyncResult<Response>.Begin(
                callback,
                state,
                beginDelegate,
                endDelegate,
                null,
                Timeout.Infinite);
        }

        public Response EndExecute(IAsyncResult asyncResult)
        {
            return Response.Success;
        }

        #endregion
    }
}
