using System;
using System.Runtime.Serialization;
using Umizoo.Messaging;

namespace UserRegistration.Events
{
    [DataContract]
    [Serializable]
    public class UserCreated : VersionedEvent
    {
        public UserCreated()
        { }

        public UserCreated(string loginId, string password, string userName, string email)
        {
            this.LoginId = loginId;
            this.Password = password;
            this.UserName = userName;
            this.Email = email;
        }

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
