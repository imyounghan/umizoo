// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Threading.Tasks;

namespace Umizoo.Messaging
{
    public interface IResultManager
    {
        int WaitingCommands { get; }

        int WaitingQueries { get; }

        Task<ICommandResult> RegisterProcessingCommand(
            string commandId,
            ICommand command,
            CommandReturnMode commandReturnMode,
            int timeoutMs);

        Task<IQueryResult> RegisterProcessingQuery(string queryId, IQuery query, int timeoutMs);

        bool SetCommandResult(string commandId, ICommandResult commandResult);

        bool SetQueryResult(string queryId, IQueryResult queryResult);
    }
}