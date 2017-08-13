using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public class QueryReceiver : KafkaReceiver<QueryDescriptor, IQuery>
    {
        public QueryReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
        }

        protected override Envelope<IQuery> Convert(QueryDescriptor descriptor, ITextSerializer serializer)
        {
            var type = Configuration.QueryTypes[descriptor.TypeName];
            var query = (IQuery) serializer.Deserialize(descriptor.Metadata, type);

            var envelope = new Envelope<IQuery>(query, descriptor.TraceId);
            envelope.Items[StandardMetadata.TraceInfo] = new TraceInfo(descriptor.TraceId, descriptor.TraceAddress);
            return envelope;
        }
    }
}