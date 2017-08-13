// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.


using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging.Handling
{
    public class ResultNotifyHandler : IEnvelopedMessageHandler<QueryResult>,
        IEnvelopedMessageHandler<CommandResult>
    {
        private readonly IResultManager _resultManager;

        public ResultNotifyHandler(IResultManager resultManager)
        {
            _resultManager = resultManager;
        }

        public void Handle(Envelope<QueryResult> envelope)
        {
            if (_resultManager.SetQueryResult(envelope.MessageId, envelope.Body)) {
                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat("query({0}) is completed.", envelope.MessageId);
                }
            }
        }

        public void Handle(Envelope<CommandResult> envelope)
        {
            if (_resultManager.SetCommandResult(envelope.MessageId, envelope.Body)) {
                if (LogManager.Default.IsDebugEnabled) {
                    LogManager.Default.DebugFormat("command({0}) is completed.", envelope.MessageId);
                }
            }
        }
    }
}