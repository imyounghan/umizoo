

namespace Umizoo.Messaging.Handling
{
    using System.Collections.Generic;
    using System.Reflection;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure.Composition;

    public class PublishableExceptionConsumer : MessageConsumer<IPublishableException>
    {
        public PublishableExceptionConsumer(IMessageReceiver<Envelope<IPublishableException>> eventReceiver)
            : base(eventReceiver)
        {
        }

        public override void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies)
        {
            foreach(var exceptionType in Configuration.Current.PublishableExceptionTypes.Values)
            {
                this.Initialize(container, exceptionType);
            }
        }
    }
}
