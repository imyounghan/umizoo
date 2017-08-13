// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


namespace Umizoo.Infrastructure
{
    /// <summary>
    ///     <see cref="IProcessor" /> 的抽象实现类
    /// </summary>
    public abstract class Processor : DisposableObject, IProcessor
    {
        private readonly object lockObject = new object();

        private bool started;


        #region IProcessor 成员

        /// <summary>
        ///     启动程序
        /// </summary>
        protected abstract void Start();

        /// <summary>
        ///     停止程序
        /// </summary>
        protected abstract void Stop();

        #endregion

        #region IProcessor 成员

        void IProcessor.Start()
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                if (!started)
                {
                    Start();
                    started = true;
                }
            }
        }

        void IProcessor.Stop()
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                if (started)
                {
                    Stop();
                    started = false;
                }
            }
        }

        #endregion
    }
}