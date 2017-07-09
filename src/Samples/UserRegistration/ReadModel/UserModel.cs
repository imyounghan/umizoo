using System;
using System.Runtime.Serialization;

namespace UserRegistration.ReadModel
{
    [DataContract]
    public class UserModel
    {
        public UserModel()
        { }

        public UserModel(string loginId)
        {
            this.LoginId = loginId;
        }

        [DataMember(Name = "userId")]
        public string UserID { get; set; }
        [DataMember]
        public string LoginId { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string UserName { get; set; }

        public override bool Equals(object obj)
        {
            var other  = obj as UserModel;
            if (other == null)
                return false;

            return this.LoginId == other.LoginId;
        }

        public override int GetHashCode()
        {
            return this.LoginId.GetHashCode();
        }
    }
}
