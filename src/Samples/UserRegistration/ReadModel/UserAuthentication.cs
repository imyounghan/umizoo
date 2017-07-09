

namespace UserRegistration.ReadModel
{
    using System.Runtime.Serialization;

    using Umizoo.Infrastructure;
    using Umizoo.Messaging;

    [DataContract]
    public class UserAuthentication : MessageBase, IQuery
    {
        [DataMember]
        public string LoginId { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string IpAddress { get; set; }
    }
}
