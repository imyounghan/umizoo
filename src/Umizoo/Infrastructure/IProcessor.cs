

namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示这是一个任务
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// 启动任务
        /// </summary>
        void Start();

        /// <summary>
        /// 停止任务
        /// </summary>
        void Stop();
    }
}
