

namespace Umizoo.Infrastructure
{
    /// <summary>
    /// <see cref="IProcessor"/> 的抽象实现类
    /// </summary>
    public abstract class Processor : DisposableObject, IProcessor
    {
        /// <summary>
        /// 用于锁的对象
        /// </summary>
        private readonly object lockObject;


        private bool started;

        protected Processor()
        {
            this.lockObject = new object();
        }

        #region IProcessor 成员
        /// <summary>
        /// 启动程序
        /// </summary>
        protected abstract void Start();

        /// <summary>
        /// 停止程序
        /// </summary>
        protected abstract void Stop();

        #endregion

        #region IProcessor 成员

        void IProcessor.Start()
        {
            ThrowIfDisposed();

            lock(this.lockObject) {
                if(!this.started) {
                    this.Start();
                    this.started = true;
                }
            }
        }

        void IProcessor.Stop()
        {
            ThrowIfDisposed();

            lock (this.lockObject) {
                if(this.started) {
                    this.Stop();
                    this.started = false;
                }
            }
        }

        #endregion
    }
}
