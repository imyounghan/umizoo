using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    [DataContract]
    public class CommandDescriptor : QueryDescriptor
    {
        public CommandDescriptor()
        {
        }

        public CommandDescriptor(ICommand command)
        {
            var keyProvider = command as IRoutingProvider;
            if (keyProvider != null) Key = keyProvider.GetRoutingKey();
            else Key = string.Empty;
            TypeName = command.GetType().Name;
        }

        [IgnoreDataMember]
        [ScriptIgnore]
        public string Key { get; set; }

        [DataMember(Name = "id")]
        public string CommandId { get; set; }

        public override string GetKey()
        {
            return Key;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})#{2}@{3}", TypeName, Metadata, TraceId, TraceAddress);
        }
    }
}