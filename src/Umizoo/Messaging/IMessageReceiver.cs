

namespace Umizoo.Messaging
{
    using System;

    /// <summary>
    /// 表示
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public interface IMessageReceiver<TEventArgs> where TEventArgs : EventArgs
    {
        event EventHandler<TEventArgs> MessageReceived;


        /// <summary>
        /// Starts the listener.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the listener.
        /// </summary>
        void Stop();
    }
}
