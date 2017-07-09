namespace Umizoo.Infrastructure
{
    using System;
    using System.Runtime.ConstrainedExecution;

    /// <summary>
    /// 表示派生对象是需要释放资源的
    /// </summary>
    public abstract class DisposableObject : CriticalFinalizerObject, IDisposable
    {
        /// <summary>
        /// 析构函数
        /// </summary>
        ~DisposableObject()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">表示对象是否应该明确处理</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);

            this.disposed = true;
        }

        private bool disposed;

        /// <summary>
        /// 表示该对象已释放的异常
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if(this.disposed)
                throw new ObjectDisposedException(this.GetType().FullName);
        }
    }
}
