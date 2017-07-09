

namespace Umizoo.Infrastructure
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class MessageBase
    {
        public override string ToString()
        {
            return string.Format("{0}({1})",
                this.GetType().FullName,
                DefaultTextSerializer.Instance.Serialize(this));
        }
    }
}
