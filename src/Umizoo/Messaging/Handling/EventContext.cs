

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;

    using Umizoo.Infrastructure;

    public class EventContext : IEventContext
    {
        private readonly List<ICommand> commands;

        private readonly IMessageBus<ICommand> commandBus;
        private readonly IMessageBus<IResult> resultBus;

        private bool replied;


        public EventContext(IMessageBus<ICommand> commandBus, IMessageBus<IResult> resultBus)
        {
            this.commandBus = commandBus;
            this.resultBus = resultBus;

            this.commands = new List<ICommand>();
        }

        public SourceInfo SourceInfo { get; set; }

        public SourceInfo CommandInfo { get; set; }

        public TraceInfo TraceInfo { get; set; }

        public void AddCommand(ICommand command)
        {
            this.commands.Add(command);
        }

        public void Commit()
        {
            if(!this.commands.IsEmpty()) {
                commandBus.Send(this.commands, this.TraceInfo);
                return;
            }

            if (!this.replied) {
                resultBus.Send(CommandResult.EventHandled, this.TraceInfo);
            }
        }

        #region IEventContext 成员


        public void CompleteCommand(object result, Func<object, string> serializer)
        {
            if(replied) {
                return;
            }

            this.replied = true;

            if (result == null || result == DBNull.Value)
            {
                resultBus.Send(CommandResult.ManualCompleted, this.TraceInfo);
                return;
            }

            Ensure.NotNull(serializer, "serializer");

            var commandResult = new CommandResult() {
                Result = serializer(result),
                ReplyType = CommandReturnMode.Manual,
            };
            resultBus.Send(commandResult, this.TraceInfo);
        }

        #endregion
    }
}
