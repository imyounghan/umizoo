// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-06.

using System;
using System.Runtime.ConstrainedExecution;

namespace Umizoo.Infrastructure
{
    /// <summary>
    ///     表示派生对象是需要释放资源的
    /// </summary>
    public abstract class DisposableObject : CriticalFinalizerObject, IDisposable
    {
        private bool _disposed;

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            _disposed = true;
        }

        /// <summary>
        ///     析构函数
        /// </summary>
        ~DisposableObject()
        {
            Dispose(false);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <param name="disposing">表示对象是否应该明确处理</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        ///     表示该对象已释放的异常
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}