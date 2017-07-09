

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 表示继承该接口的是一个命令处理程序
    /// </summary>
    public interface ICommandHandler : IHandler
    { }

    /// <summary>
    /// 表示继承该接口的是一个命令处理程序
    /// </summary>
    public interface ICommandHandler<TCommand> : ICommandHandler
        where TCommand : class, ICommand
    {
        /// <summary>
        /// 处理命令
        /// </summary>
        void Handle(ICommandContext context, TCommand command);
    }
}
