
namespace Umizoo.Messaging
{
    public interface IEventPublishedVersionStore
    {
        /// <summary>
        /// 更新版本号
        /// </summary>
        void AddOrUpdatePublishedVersion(SourceInfo sourceInfo, int version);

        /// <summary>
        /// 获取已发布的版本号
        /// </summary>
        int GetPublishedVersion(SourceInfo sourceInfo);
    }
}
