
namespace Umizoo.Messaging.Handling
{
    public class ResultConsumerWithLocal : ResultConsumer
    {
        private readonly IResultManager resultManager;
        public ResultConsumerWithLocal(IMessageReceiver<Envelope<IResult>> resultReceiver, IResultManager resultManager)
            : base(resultReceiver)
        {
            this.resultManager = resultManager;
        }

        protected override void Dispose(bool disposing)
        {

        }

        protected override void OnResultReceived(object sender, Envelope<IResult> envelope)
        {
            var commandResult = envelope.Body as CommandResult;
            if(commandResult != null) {
                //if(LogManager.Default.IsDebugEnabled) {
                //    LogManager.Default.DebugFormat(
                //        "Receive a command result. CommandId:{0},Status:{1},ErrorCode:{2},ErrorMessage:{3},ReturnMode:{4},From:{5}.",
                //        envelope.MessageId,
                //        commandResult.Status,
                //        commandResult.ErrorCode,
                //        commandResult.ErrorMessage,
                //        commandResult.ReplyType,
                //        commandResult.ReplyServer);
                //}

                if(resultManager.SetCommandResult(envelope.MessageId, commandResult)) {
                    if(LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.DebugFormat("Command:{0} is completed.", envelope.MessageId);
                    }
                }
                return;
            }

            var queryResult = envelope.Body as QueryResult;
            if(queryResult != null) {
                //if(LogManager.Default.IsDebugEnabled) {
                //    LogManager.Default.DebugFormat(
                //        "Receive a query result. QueryId:{0},Status:{1},ErrorMessage:{2},From:{3}.",
                //        envelope.MessageId,
                //        queryResult.Status,
                //        queryResult.ErrorMessage,
                //        queryResult.ReplyServer);
                //}

                if(resultManager.SetQueryResult(envelope.MessageId, queryResult)) {
                    if(LogManager.Default.IsDebugEnabled) {
                        LogManager.Default.DebugFormat("Query:{0} is completed.", envelope.MessageId);
                    }
                }
                return;
            }

            LogManager.Default.Warn("Unknown Reply.");
        }
    }
}
