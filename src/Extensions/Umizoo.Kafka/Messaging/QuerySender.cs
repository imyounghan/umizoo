

namespace Umizoo.Messaging
{
    using Umizoo.Configurations;
    using Umizoo.Infrastructure;

    public class QuerySender : KafkaSender<IQuery, QueryDescriptor>
    {
        public QuerySender(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
        }

        protected override QueryDescriptor Convert(Envelope<IQuery> envelope, ITextSerializer serializer)
        {
            var descriptor = new QueryDescriptor() {
                TypeName = envelope.Body.GetType().Name,
                Metadata = serializer.Serialize(envelope.Body)
            };
            if(envelope.Items.ContainsKey(StandardMetadata.TraceInfo)) {
                var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];
                descriptor.TraceId = traceInfo.Id;
                descriptor.TraceAddress = traceInfo.Address;
            }

            return descriptor;
        }

        protected override string GetLogInfo(QueryDescriptor descriptor)
        {
            return string.Format("{0}({1})#{2}", 
                Configuration.Current.QueryTypes[descriptor.TypeName].GetFullName(),
                descriptor.Metadata, descriptor.TraceId);
        }
    }
}
