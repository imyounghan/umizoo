

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
    public class WcfRequestServer : CentralService, IContract, IProcessor
    {
        private readonly ITextSerializer serializer;
        private readonly object lockObject;

        public WcfRequestServer(
            IMessageBus<ICommand> commandBus,
            IMessageBus<IQuery> queryBus,
            ITextSerializer serializer,
            IResultManager resultManger)
            : base(commandBus, queryBus, resultManger)
        {
            this.serializer = serializer;
            this.lockObject = new object();
        }


        #region IProcessor 成员

        private ServiceHost wcfHost;

        private bool started;

        private void Start()
        {
            wcfHost = new ServiceHost(this);
            wcfHost.Opened += (sender, e) => Console.WriteLine("WCF Request Service Started.");
            wcfHost.Open();
        }

        private void Stop()
        {
            using (wcfHost) {
                wcfHost.Close();
                wcfHost = null;
            }
        }

        void IProcessor.Start()
        {
            lock (this.lockObject) {
                if (!this.started) {
                    this.Start();
                    this.started = true;
                }
            }
        }

        void IProcessor.Stop()
        {
            lock (this.lockObject) {
                if (this.started) {
                    this.Stop();
                    this.started = false;
                }
            }
        }

        #endregion

        private IAsyncResult CreateInnerAsyncResult(AsyncCallback asyncCallback, object asyncState)
        {
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(asyncState);
            asyncResult.MarkCompleted(true /* completedSynchronously */, asyncCallback);
            return asyncResult;
        }

        /// <summary>
        /// 发送一个请求异步返回结果
        /// </summary>
        /// <param name="request">一个请求信息</param>
        /// <param name="callback">回调函数</param>
        /// <param name="state">状态</param>
        /// <returns>异步结果</returns>
        public IAsyncResult BeginExecute(Request request, AsyncCallback callback, object state)
        {
            TaskCompletionSource<Response> responseTask = new TaskCompletionSource<Response>();

            BeginInvokeDelegate beginDelegate = delegate (AsyncCallback asyncCallback, object asyncState) {

                if (this.WaitingRequests > ConfigurationSettings.MaxRequests) {
                    responseTask.SetResult(Response.ServerTooBusy);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                int protocol = request.Header.GetIfKeyNotFound("Protocol").ChangeIfError(0);
                string typeName = request.Header.GetIfKeyNotFound("Type");
                Type type = null;
                bool typeConfirmed = false;
                switch ((ProtocolCode)protocol) {
                    case ProtocolCode.Command:
                        typeConfirmed = Configuration.Current.CommandTypes.TryGetValue(typeName, out type);
                        break;
                    case ProtocolCode.Query:
                        typeConfirmed = Configuration.Current.QueryTypes.TryGetValue(typeName, out type);
                        break;
                }

                if (!typeConfirmed) {
                    responseTask.SetResult(Response.UnknownType);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                object graph;
                try {
                    graph = serializer.Deserialize(request.Body, type);
                }
                catch (Exception) {
                    responseTask.TrySetResult(Response.ParsingFailure);
                    return CreateInnerAsyncResult(asyncCallback, asyncState);
                }

                int timeout = request.Header.GetIfKeyNotFound("Timeout", "0").ChangeIfError(0);
                IResult result = null;
                switch ((ProtocolCode)protocol) {
                    case ProtocolCode.Command:
                        var returnMode = (CommandReturnMode)request.Header.GetIfKeyNotFound("Mode", "1").ChangeIfError(1);
                        result = this.ExecuteAsync((ICommand)graph, returnMode, timeout).Result;
                        break;
                    case ProtocolCode.Query:
                        result = this.FetchAsync((IQuery)graph, timeout).Result;
                        break;
                }

                var message = serializer.Serialize(result);
                responseTask.TrySetResult(new Response(200, message));


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
            return WrappedAsyncResult<Response>.Cast(asyncResult, null).End();
        }
    }
}
