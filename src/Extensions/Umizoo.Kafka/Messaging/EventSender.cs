using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public class EventSender : KafkaSender<IEvent, EventDescriptor>
    {
        public EventSender(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
        }

        protected override EventDescriptor Convert(Envelope<IEvent> envelope, ITextSerializer serializer)
        {
            var descriptor = new EventDescriptor
            {
                EventId = envelope.MessageId,
                TypeName = envelope.Body.GetType().Name,
                Metadata = serializer.Serialize(envelope.Body)
            };
            if (envelope.Items.ContainsKey(StandardMetadata.CommandInfo))
            {
                var commandInfo = (SourceInfo) envelope.Items[StandardMetadata.CommandInfo];
                descriptor.CommandId = commandInfo.Id;
                descriptor.CommandTypeName = commandInfo.TypeName;
            }
            if (envelope.Items.ContainsKey(StandardMetadata.SourceInfo))
            {
                var sourceInfo = (SourceInfo) envelope.Items[StandardMetadata.SourceInfo];
                descriptor.SourceId = sourceInfo.Id;
                descriptor.SourceTypeName = sourceInfo.TypeName;
            }
            if (envelope.Items.ContainsKey(StandardMetadata.TraceInfo))
            {
                var traceInfo = (TraceInfo) envelope.Items[StandardMetadata.TraceInfo];
                descriptor.TraceId = traceInfo.Id;
                descriptor.TraceAddress = traceInfo.Address;
            }


            return descriptor;
        }

        protected override string GetLogInfo(EventDescriptor descriptor)
        {
            return string.Format("{0}({1})#{2}&{3}@{4}",
                Configuration.EventTypes[descriptor.TypeName].GetFullName(),
                descriptor.Metadata, descriptor.EventId,
                descriptor.SourceId.IfEmpty("null"),
                descriptor.CommandId.IfEmpty("null"));
        }
    }
}