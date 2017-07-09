using System.Runtime.Serialization;
using Umizoo.Infrastructure;
using Umizoo.Messaging;

namespace UserRegistration.ReadModel
{
    [DataContract]
    public class FindAllUser : MessageBase, IQuery
    { }
}
