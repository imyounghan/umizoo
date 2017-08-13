using System;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Configurations;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Composition;
using Umizoo.Seeds;

namespace Umizoo.Messaging
{
    public class EventReceiver : KafkaReceiver<EventDescriptor, IEvent>
    {
        public EventReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
        }

        protected override Envelope<IEvent> Convert(EventDescriptor descriptor, ITextSerializer serializer)
        {
            var type = Configuration.EventTypes[descriptor.TypeName];
            var @event = (IEvent) serializer.Deserialize(descriptor.Metadata, type);

            var envelope = new Envelope<IEvent>(@event, descriptor.EventId);
            if (!string.IsNullOrEmpty(descriptor.CommandId))
                envelope.Items[StandardMetadata.CommandInfo] =
                    new SourceInfo(descriptor.CommandId, Configuration.CommandTypes[descriptor.CommandTypeName]);
            if (!string.IsNullOrEmpty(descriptor.SourceId))
                envelope.Items[StandardMetadata.SourceInfo] =
                    new SourceInfo(descriptor.SourceId, Configuration.AggregateTypes[descriptor.SourceTypeName]);
            if (!string.IsNullOrEmpty(descriptor.TraceId))
                envelope.Items[StandardMetadata.TraceInfo] = new TraceInfo(descriptor.TraceId, descriptor.TraceAddress);

            return envelope;
        }
    }
}