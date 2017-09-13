// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System.Collections.Generic;

namespace Umizoo.Messaging.Handling
{
    public class EventContext : IEventContext
    {
        private readonly IMessageBus<ICommand> _commandBus;
        private readonly IMessageBus<IResult> _resultBus;
        private readonly List<ICommand> _commands;


        public EventContext(IMessageBus<ICommand> commandBus, IMessageBus<IResult> resultBus)
        {
            _commandBus = commandBus;
            _resultBus = resultBus;
            _commands = new List<ICommand>();
        }

        public TraceInfo TraceInfo { get; set; }

        public SourceInfo SourceInfo { get; set; }

        public SourceInfo CommandInfo { get; set; }

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        //public void CompleteCommand(object result, Func<object, string> serializer)
        //{
        //    if (_replied) return;


        //    var commandResult = CommandResult.Finished;
        //    if (!result.IsNull())
        //        if (!serializer.IsNull())
        //            commandResult = new CommandResult
        //            {
        //                Result = serializer(result),
        //                ResultType = result.GetType().GetFriendlyTypeName(),
        //                ReplyType = CommandReturnMode.Manual
        //            };
        //    _resultBus.Send(commandResult, TraceInfo);

        //    _replied = true;
        //}

        private bool _committed;
        public void Commit()
        {
            if (_committed) {
                return;
            }

            if (!_commands.IsEmpty())
            {
                _commandBus.Send(_commands, TraceInfo);
            }
            else {
                _resultBus.Send(CommandResult.EventHandled, TraceInfo);
            }

            _committed = true;
        }
    }
}