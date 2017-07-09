
namespace Umizoo.Messaging
{
    using Umizoo.Configurations;
    using Umizoo.Infrastructure;

    public class CommandSender : KafkaSender<ICommand, CommandDescriptor>
    {
        public CommandSender(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        { }

        protected override CommandDescriptor Convert(Envelope<ICommand> envelope, ITextSerializer serializer)
        {
            var descriptor = new CommandDescriptor(envelope.Body) {
                CommandId = envelope.MessageId,
                Metadata = serializer.Serialize(envelope.Body)
            };
            if(envelope.Items.ContainsKey(StandardMetadata.TraceInfo)) {
                var traceInfo = (TraceInfo)envelope.Items[StandardMetadata.TraceInfo];
                descriptor.TraceId = traceInfo.Id;
                descriptor.TraceAddress = traceInfo.Address;
            }

            return descriptor;
        }

        protected override string GetLogInfo(CommandDescriptor descriptor)
        {
            return string.Format("{0}({1})#{2}", 
                Configuration.Current.CommandTypes[descriptor.TypeName].GetFullName(),
                descriptor.Metadata, descriptor.CommandId);
        }
    }
}
