
namespace Umizoo.Messaging
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Umizoo.Configurations;
    using Umizoo.Infrastructure;

    public class PublishableExceptionSender : KafkaSender<IPublishableException, PublishableExceptionDescriptor>
    {
        static string[] IgnoreExceptionProperties =
            {
                "ClassName",
                "Data",
                "InnerException",
                "HelpURL",
                "Source",
                "StackTrace",
                "TargetSite",
                "StackTraceString",
                "RemoteStackTraceString",
                "RemoteStackIndex",
                "ExceptionMethod",
                "WatsonBuckets"
            };

        public PublishableExceptionSender(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
        }

        protected override PublishableExceptionDescriptor Convert(Envelope<IPublishableException> envelope, ITextSerializer serializer)
        {
            var type = envelope.Body.GetType();

            var descriptor = new PublishableExceptionDescriptor(envelope.Body) {
                ExceptionId = envelope.MessageId,
                TypeName = type.FullName
            };

            var serializableInfo = new SerializationInfo(type, new FormatterConverter());

            List<PublishableExceptionDescriptor.PropertyEntry> items = new List<PublishableExceptionDescriptor.PropertyEntry>();
            envelope.Body.GetObjectData(serializableInfo, new StreamingContext(StreamingContextStates.Clone));

            for(SerializationInfoEnumerator enumerator = serializableInfo.GetEnumerator(); enumerator.MoveNext(); ) {
                if(enumerator.Current.Name.InArray(IgnoreExceptionProperties))
                    continue;

                var serializationEntry = new PublishableExceptionDescriptor.PropertyEntry(enumerator.Current.ObjectType) {
                    Value = enumerator.Current.Value.ToString(),
                    Key = enumerator.Current.Name
                };
                items.Add(serializationEntry);
            }
            descriptor.Items = items;

            return descriptor;
        }

        protected override string GetLogInfo(PublishableExceptionDescriptor descriptor)
        {
            var items = descriptor.Items.Select(item => string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));
            var metadata = string.Concat("{", string.Join(",", items), "}");

            return string.Format("{0}({1})#{2}", 
                Configuration.Current.PublishableExceptionTypes[descriptor.TypeName].GetFullName(),
                metadata, descriptor.ExceptionId);
        }
    }
}
