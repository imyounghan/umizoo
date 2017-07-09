namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 表示继承该接口的是一个处理程序
    /// </summary>
    public interface IMessageHandler<TMessage> : IHandler
        where TMessage : class, IMessage
    {
        /// <summary>
        /// 处理消息。
        /// </summary>
        void Handle(TMessage message);
    }
}
