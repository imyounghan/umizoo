using System;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public abstract class ResultConsumer : Processor
    {
        private readonly IMessageReceiver<Envelope<IResult>> resultReceiver;

        public ResultConsumer(IMessageReceiver<Envelope<IResult>> resultReceiver)
        {
            this.resultReceiver = resultReceiver;
        }

        protected abstract void OnResultReceived(object sender, Envelope<IResult> envelope);

        protected override void Start()
        {
            this.resultReceiver.MessageReceived += this.OnResultReceived;
            this.resultReceiver.Start();

            LogManager.Default.InfoFormat("Result Consumer Started!");
        }

        protected override void Stop()
        {
            this.resultReceiver.MessageReceived -= this.OnResultReceived;
            this.resultReceiver.Start();

            LogManager.Default.InfoFormat("Result Consumer Started!");
        }
    }

    public abstract class ReplyResultConsumer : ResultConsumer
    {
        public ReplyResultConsumer(IMessageReceiver<Envelope<IResult>> resultReceiver)
            : base(resultReceiver)
        {
        }


        protected abstract bool SendReply(IResult result, TraceInfo traceInfo);

        protected override void OnResultReceived(object sender, Envelope<IResult> envelope)
        {
            var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];


            bool success;
            try {
                success = this.SendReply(envelope.Body, traceInfo);
            }
            catch(Exception ex) {
                LogManager.Default.Error(ex, 
                    "Send reply result has exeption, result:({0}),traceId:{1},address:{2}.", 
                    envelope.Body, traceInfo.Id, traceInfo.Address);
                success = false;
            }

            if(!success) {
                var resultBus = sender as IMessageBus<IResult>;
                if(resultBus != null) {
                    resultBus.Send(envelope);
                }
            }
        }
    }
}
