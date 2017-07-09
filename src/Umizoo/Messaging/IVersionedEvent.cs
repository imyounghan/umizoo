
namespace Umizoo.Messaging
{
    /// <summary>
    /// 表示继承该接口的是一个有序事件
    /// </summary>
    public interface IVersionedEvent : IEvent
    {
        /// <summary>
        /// Gets the version or order of the event in the stream.
        /// </summary>
        int Version { get; }
    }
}
