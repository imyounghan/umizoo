// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Threading.Tasks;

using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;


namespace Umizoo.Messaging
{
    public class CommandService : ICommandService
    {
        private readonly IMessageBus<ICommand> _commandBus;
        private readonly IResultManager _resultManger;

        public CommandService(IMessageBus<ICommand> commandBus, IResultManager resultManger)
        {
            _commandBus = commandBus;
            _resultManger = resultManger;
        }

        public int WaitingRequests { get { return _resultManger.WaitingCommands; }}

        public ICommandResult Execute(ICommand command, CommandReturnMode returnMode, int timeoutMs)
        {
            return ExecuteAsync(command, returnMode, timeoutMs).Result;
        }


        public Task<ICommandResult> ExecuteAsync(ICommand command, CommandReturnMode returnMode, int timeoutMs)
        {
            var traceId = ObjectId.GenerateNewId().ToString();
            var commandTask = _resultManger.RegisterProcessingCommand(traceId, command, returnMode, timeoutMs);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    _commandBus.Send(command, new TraceInfo(traceId, ConfigurationSettings.InnerAddress));
                    _resultManger.SetCommandResult(traceId, CommandResult.SentSuccess);
                }
                catch (Exception ex)
                {
                    LogManager.Default.Error("send commands failed. ", ex);
                    _resultManger.SetCommandResult(traceId, CommandResult.SentFailed);
                }
            });

            return commandTask;
        }
    }
}