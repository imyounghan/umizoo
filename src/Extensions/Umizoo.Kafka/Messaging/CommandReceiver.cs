using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public class CommandReceiver : KafkaReceiver<CommandDescriptor, ICommand>
    {
        public CommandReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
        }

        protected override Envelope<ICommand> Convert(CommandDescriptor descriptor, ITextSerializer serializer)
        {
            var type = Configuration.CommandTypes[descriptor.TypeName];
            var command = (ICommand)serializer.Deserialize(descriptor.Metadata, type);

            var envelope = new Envelope<ICommand>(command, descriptor.CommandId);
            envelope.Items[StandardMetadata.TraceInfo] = new TraceInfo(descriptor.TraceId, descriptor.TraceAddress);
            return envelope;
        }
    }
}