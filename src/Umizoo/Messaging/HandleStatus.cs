

namespace Umizoo.Messaging
{
    /// <summary>
    /// 返回状态枚举定义
    /// </summary>
    public enum HandleStatus : int
    {
        /// <summary>
        /// 错误
        /// </summary>
        Failed = 0,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 同步数据错误
        /// </summary>
        SyncFailed = 2,        
        /// <summary>
        /// 没有变化或数据
        /// </summary>
        Nothing = 3,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout = 4,
    }
}
