// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections.Generic;

using Umizoo.Communication;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public class ResultReplyHandler : IEnvelopedMessageHandler<QueryResult>,
        IEnvelopedMessageHandler<CommandResult>
    {
        private readonly ITextSerializer _serializer;
        private readonly IClientChannelFactory _channelFactory;

        public ResultReplyHandler(ITextSerializer serializer, IClientChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
            _serializer = serializer;
        }

        public void Handle(Envelope<QueryResult> envelope)
        {
            var traceInfo = envelope.Items[StandardMetadata.TraceInfo] as TraceInfo;
            Assertions.NotNull(traceInfo, "traceInfo");

            var request = new Request() {
                Body = _serializer.Serialize(envelope.Body),
                Header = new Dictionary<string, string>()
                {
                    { "Type", envelope.Body.GetType().Name },
                    { "TraceId", traceInfo.Id },
                }
            };

            var channel = _channelFactory.GetChannel(traceInfo.Address, ProtocolCode.Query);

            channel.BeginExecute(request, null, null);
        }

        public void Handle(Envelope<CommandResult> envelope)
        {
            var traceInfo = envelope.Items[StandardMetadata.TraceInfo] as TraceInfo;
            Assertions.NotNull(traceInfo, "traceInfo");

            var request = new Request() {
                Body = _serializer.Serialize(envelope.Body),
                Header = new Dictionary<string, string>()
                {
                    { "Type", envelope.Body.GetType().Name },
                    { "TraceId", traceInfo.Id },
                }
            };

            var channel = _channelFactory.GetChannel(traceInfo.Address, ProtocolCode.Command);

            channel.BeginExecute(request, null, null);
        }
    }
}