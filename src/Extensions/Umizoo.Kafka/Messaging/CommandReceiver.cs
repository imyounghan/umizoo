
namespace Umizoo.Messaging
{
    using Umizoo.Configurations;
    using Umizoo.Infrastructure;

    public class CommandReceiver : KafkaReceiver<CommandDescriptor, ICommand>
    {
        public CommandReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {        }

        protected override Envelope<ICommand> Convert(CommandDescriptor descriptor)
        {
            var type = Configuration.Current.CommandTypes[descriptor.TypeName];
            var command = (ICommand)_serializer.Deserialize(descriptor.Metadata, type);

            var envelope = new Envelope<ICommand>(command, descriptor.CommandId);
            envelope.Items[StandardMetadata.TraceInfo] = new TraceInfo(descriptor.TraceId, descriptor.TraceAddress);
            return envelope;
        }
    }
}
