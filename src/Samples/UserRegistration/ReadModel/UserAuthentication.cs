using System.Runtime.Serialization;
using Umizoo.Messaging;

namespace UserRegistration.ReadModel
{
    [DataContract]
    public class UserAuthentication : IQuery
    {
        [DataMember]
        public string LoginId { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string IpAddress { get; set; }
    }
}