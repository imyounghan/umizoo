// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System.Threading.Tasks;

namespace Umizoo.Messaging
{
    public interface ICommandService
    {
        /// <summary>
        ///     异步执行一个命令
        /// </summary>
        Task<ICommandResult> ExecuteAsync(ICommand command,
            CommandReturnMode returnType = CommandReturnMode.CommandExecuted, int timeoutMs = 120000);

        /// <summary>
        ///     在规定时间内执行一个命令
        /// </summary>
        ICommandResult Execute(ICommand command, CommandReturnMode returnType = CommandReturnMode.CommandExecuted,
            int timeoutMs = 120000);

        int WaitingRequests { get; }
    }
}