
using System.Runtime.Serialization;
using Umizoo.Messaging;

namespace UserRegistration.Commands
{
    [DataContract]
    public class RegisterUser : Command
    {
        [DataMember]
        public string LoginId { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}
