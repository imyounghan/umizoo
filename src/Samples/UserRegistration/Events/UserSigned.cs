using System;
using System.Runtime.Serialization;
using Umizoo.Messaging;

namespace UserRegistration.Events
{
    [DataContract]
    [Serializable]
    public class UserSigned : IEvent
    {
        public UserSigned()
        { }

        public UserSigned(string loginid, string clientIp)
        {
            this.LoginId = loginid;
            this.ClientIP = clientIp;
            this.LoginTime = DateTime.UtcNow;
        }

        [DataMember]
        public string LoginId { get; private set; }
        [DataMember]
        public string ClientIP { get; private set; }
        [DataMember]
        public DateTime LoginTime { get; private set; }
    }
}
