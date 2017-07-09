

namespace Umizoo.Infrastructure
{
    using System;
    using Umizoo.Messaging;

    public class DefaultTopicProvider : ITopicProvider
    {
        public string GetTopic(Type type)
        {
            if(type == typeof(PublishableExceptionDescriptor))
                return "Exceptions";

            if(type == typeof(CommandDescriptor))
                return "Commands";

            if(type == typeof(EventDescriptor))
                return "Events";

            if(type == typeof(QueryDescriptor))
                return "Queries";

            throw new ApplicationException(string.Format("Unknown topic from the type of '{0}'.", type.FullName));
        }
    }
}
