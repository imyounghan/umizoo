

namespace Umizoo.Messaging
{
    public abstract class VersionedEvent : Event, IVersionedEvent
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; protected internal set; }
    }
}
