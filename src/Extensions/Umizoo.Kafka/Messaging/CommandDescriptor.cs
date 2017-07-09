

namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;
    using System.Web.Script.Serialization;

    using Umizoo.Infrastructure;

    [DataContract]
    public class CommandDescriptor : QueryDescriptor
    {
        public CommandDescriptor()
        { }

        public CommandDescriptor(ICommand command)
        {
            var keyProvider = command as IRoutingProvider;
            if(keyProvider != null) {
                this.Key = keyProvider.GetRoutingKey();
            }
            else {
                this.Key = string.Empty;
            }
            this.TypeName = command.GetType().Name;
        }

        [IgnoreDataMember]
        [ScriptIgnore]
        public string Key { get; set; }

        [DataMember(Name = "id")]
        public string CommandId { get; set; }

        public override string GetKey()
        {
            return this.Key;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})#{2}@{3}", TypeName, Metadata, TraceId, TraceAddress);
        }
    }
}
