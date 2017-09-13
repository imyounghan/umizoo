// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public static class MessageBusExtentions
    {
        public static void Send(this IMessageBus<ICommand> commandBus, ICommand command, TraceInfo traceInfo)
        {
            var envelope = new Envelope<ICommand>(command, ObjectId.GenerateNewId().ToString());
            envelope.Items[StandardMetadata.TraceInfo] = traceInfo;

            commandBus.Send(envelope);
        }

        public static void Send(this IMessageBus<ICommand> commandBus, IEnumerable<ICommand> commands,
            TraceInfo traceInfo)
        {
            var envelopes = commands.Select(command =>
            {
                var envelope = new Envelope<ICommand>(command, ObjectId.GenerateNewId().ToString());
                envelope.Items[StandardMetadata.TraceInfo] = traceInfo;

                return envelope;
            });

            commandBus.Send(envelopes);
        }

        public static void Publish(this IMessageBus<IEvent> eventBus, IEvent @event)
        {
            var envelope = new Envelope<IEvent>(@event, ObjectId.GenerateNewId().ToString());

            eventBus.Send(envelope);
        }

        public static void Publish(this IMessageBus<IEvent> eventBus, IEnumerable<IEvent> events)
        {
            var envelopes = events.Select(@event => new Envelope<IEvent>(@event, ObjectId.GenerateNewId().ToString())).ToArray();

            eventBus.Send(envelopes);
        }


        public static void Publish(this IMessageBus<IEvent> eventBus, SourceInfo sourceInfo,
            IEnumerable<IVersionedEvent> events, Envelope<ICommand> command)
        {
            var envelopes = events.Select(@event => Convert(@event, sourceInfo, command)).ToArray();
            eventBus.Send(envelopes);
        }

        public static void Publish(this IMessageBus<IPublishableException> exBus,
            IPublishableException publishableException)
        {
            var envelope =
                new Envelope<IPublishableException>(publishableException, ObjectId.GenerateNewId().ToString());

            exBus.Send(envelope);
        }

        private static Envelope<IEvent> Convert(IVersionedEvent @event, SourceInfo sourceInfo,
            Envelope<ICommand> command)
        {
            var envelope = new Envelope<IEvent>(@event,
                MD5(string.Format("{0}&{1}", sourceInfo.Id, command.MessageId)));
            if (command.Items.ContainsKey(StandardMetadata.TraceInfo))
                envelope.Items[StandardMetadata.TraceInfo] = command.Items[StandardMetadata.TraceInfo];
            envelope.Items[StandardMetadata.SourceInfo] = sourceInfo;
            envelope.Items[StandardMetadata.CommandInfo] = new SourceInfo(command.MessageId, command.Body.GetType());

            return envelope;
        }

        private static string MD5(string source)
        {
            var sb = new StringBuilder(32);

            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                var t = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
                for (var i = 0; i < t.Length; i++) sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        public static void Send(this IMessageBus<IResult> resultBus, IResult result, TraceInfo traceInfo)
        {
            var envelope = new Envelope<IResult>(result, traceInfo.Id);
            envelope.Items[StandardMetadata.TraceInfo] = traceInfo;

            resultBus.Send(envelope);
        }

        public static void Send(this IMessageBus<IResult> resultBus, IResult result, string traceId)
        {
            var envelope = new Envelope<IResult>(result, traceId);

            resultBus.Send(envelope);
        }

        public static void Send(this IMessageBus<IQuery> queryBus, IQuery query, TraceInfo traceInfo)
        {
            var envelope = new Envelope<IQuery>(query, traceInfo.Id);
            envelope.Items[StandardMetadata.TraceInfo] = traceInfo;

            queryBus.Send(envelope);
        }
    }
}