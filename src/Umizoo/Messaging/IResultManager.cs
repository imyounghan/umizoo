

namespace Umizoo.Messaging
{
    using System.Threading.Tasks;


    public interface IResultManager
    {
        Task<ICommandResult> RegisterProcessingCommand(
            string commandId,
            ICommand command,
            CommandReturnMode commandReturnMode,
            int timeoutMs);

        Task<IQueryResult> RegisterProcessingQuery(string queryId, IQuery query, int timeoutMs);

        bool SetCommandResult(string commandId, ICommandResult commandResult);

        bool SetQueryResult(string queryId, IQueryResult queryResult);

        int WaitingCommands { get; }

        int WaitingQueries { get; }
    }
}
