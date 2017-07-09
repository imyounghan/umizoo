

namespace Umizoo.Communication
{
    /// <summary>
    /// 协议码
    /// </summary>
    public enum ProtocolCode : byte
    {
        /// <summary>
        /// 命令
        /// </summary>
        Command = 1,

        /// <summary>
        /// 查询
        /// </summary>
        Query = 2,

        /// <summary>
        /// 通知
        /// </summary>
        Notify = 3
    }
}
