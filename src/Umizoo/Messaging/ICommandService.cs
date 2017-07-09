

namespace Umizoo.Messaging
{
    using System.Threading.Tasks;

    public interface ICommandService
    {
        /// <summary>
        /// 异步执行一个命令
        /// </summary>
        Task<ICommandResult> ExecuteAsync(ICommand command, CommandReturnMode returnType = CommandReturnMode.CommandExecuted, int timeoutMs = 120000);

        /// <summary>
        /// 在规定时间内执行一个命令
        /// </summary>
        ICommandResult Execute(ICommand command, CommandReturnMode returnType = CommandReturnMode.CommandExecuted, int timeoutMs = 120000);
    }
}
