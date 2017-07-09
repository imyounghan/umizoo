
namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Umizoo.Infrastructure;

    public class ResultManager : IResultManager
    {
        private readonly ConcurrentDictionary<string, CommandTaskCompletionSource> _commandTaskDict;
        private readonly ConcurrentDictionary<string, QueryTaskCompletionSource> _queryTaskDict;



        public ResultManager()
        {
            this._commandTaskDict = new ConcurrentDictionary<string, CommandTaskCompletionSource>();
            this._queryTaskDict = new ConcurrentDictionary<string, QueryTaskCompletionSource>();
        }

        private void RemoveCommandTask(string commandId)
        {
            _commandTaskDict.TryRemove(commandId);
        }

        public Task<ICommandResult> RegisterProcessingCommand(string commandId, ICommand command, CommandReturnMode commandReturnMode, int timeoutMs)
        {
            var taskCompletionSource = new CommandTaskCompletionSource(commandReturnMode, timeoutMs, commandId, RemoveCommandTask);
            if(!_commandTaskDict.TryAdd(commandId, taskCompletionSource)) {
                taskCompletionSource.Dispose();
                throw new ApplicationException(string.Format("Duplicate processing command registration, type:{0}, id:{1}", command.GetType().Name, commandId));
            }

            return taskCompletionSource.Task;
        }

        private void RemoveQueryTask(string queryId)
        {
            _queryTaskDict.TryRemove(queryId);
        }

        public Task<IQueryResult> RegisterProcessingQuery(string queryId, IQuery query, int timeoutMs)
        {
            var taskCompletionSource = new QueryTaskCompletionSource(timeoutMs, queryId, RemoveQueryTask);
            if (!_queryTaskDict.TryAdd(queryId, taskCompletionSource)) {
                taskCompletionSource.Dispose();
                throw new ApplicationException(string.Format("Duplicate processing query registration, type:{0}, id:{1}", query.GetType().Name, queryId));
            }

            return taskCompletionSource.Task;
        }
        
        public int WaitingCommands
        {
            get { return _commandTaskDict.Count; }
        }

        public int WaitingQueries
        {
            get { return _queryTaskDict.Count; }
        }

        abstract class WrappedTaskCompletionSource<T1,T2> : DisposableObject
            where T2 : class, T1
        {
            private readonly TaskCompletionSource<T1> taskCompletionSource;
            private readonly SingleEntryGate setResultGate;
            private readonly Action<string> callback;

            private readonly string sourceId;
            private readonly Timer timer;

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                    }
                    taskCompletionSource.TrySetCanceled();
                }
            }

            protected abstract T1 TimeoutResult { get; }

            private void HandleTimeout(object state)
            {
                if(!setResultGate.TryEnter()) {
                    return;
                }

                taskCompletionSource.SetResult(this.TimeoutResult);

                if(this.callback != null) {
                    callback(sourceId);
                }

                timer.Dispose();
            }

            public WrappedTaskCompletionSource(int timeoutMs, string sourceId, Action<string> callback)
            {
                this.taskCompletionSource = new TaskCompletionSource<T1>();
                this.setResultGate = new SingleEntryGate();
                this.sourceId = sourceId;
                this.callback = callback;
                if(timeoutMs > 0) {
                    this.timer = new Timer(HandleTimeout, null, timeoutMs, Timeout.Infinite);
                }
            }

            protected virtual bool CanSetResult(T2 result)
            {
                return true;
            }

            public bool TrySetResult(T2 result)
            {
                if (!this.CanSetResult(result))
                {
                    return false;
                }

                if(!setResultGate.TryEnter()) {
                    return false;
                }

                if(this.callback != null) {
                    callback(sourceId);
                }
                if(timer != null) {
                    timer.Dispose();
                }

                return taskCompletionSource.TrySetResult(result);
            }

            public Task<T1> Task
            {
                get { return taskCompletionSource.Task; }
            }
        }

        class QueryTaskCompletionSource : WrappedTaskCompletionSource<IQueryResult, QueryResult>
        {
            public QueryTaskCompletionSource(int timeoutMs, string queryId, Action<string> callback)
                : base(timeoutMs, queryId, callback)
            {
            }

            protected override IQueryResult TimeoutResult
            {
                get
                {
                    return QueryResult.Timeout;
                }
            }
        }

        class CommandTaskCompletionSource : WrappedTaskCompletionSource<ICommandResult, CommandResult>
        {
            public CommandTaskCompletionSource(CommandReturnMode returnMode, int timeoutMs, string commandId, Action<string> callback)
                : base(timeoutMs, commandId, callback)
            {
                this.ReturnMode = returnMode;
            }

            protected override ICommandResult TimeoutResult
            {
                get
                {
                    return CommandResult.Timeout;
                }
            }

            public CommandReturnMode ReturnMode { get; set; }

            public int EventCount { get; set; }
            
            protected override bool CanSetResult(CommandResult result)
            {
                if(result.Status != HandleStatus.Success || result.ReplyType == CommandReturnMode.Manual) {
                    return true;
                }

                bool completed = false;
                switch(result.ReplyType) {
                    case CommandReturnMode.CommandExecuted:
                        if(this.ReturnMode == CommandReturnMode.CommandExecuted) {
                            completed = true;
                        }
                        else {
                            this.EventCount = result.ProduceEventCount;
                        }
                        break;
                    case CommandReturnMode.EventHandled:
                        completed = --this.EventCount <= 0;
                        break;
                    case CommandReturnMode.Delivered:
                        completed = this.ReturnMode == CommandReturnMode.Delivered;
                        break;
                }

                return completed;
            }
        }

        #region IResultManager 成员


        public bool SetCommandResult(string commandId, ICommandResult commandResult)
        {
            var target = commandResult as CommandResult;
            if (target != null)
            {
                CommandTaskCompletionSource commandTaskCompletionSource;
                if (_commandTaskDict.TryGetValue(commandId, out commandTaskCompletionSource))
                {
                    return commandTaskCompletionSource.TrySetResult(target);
                }
            }

            return false;
        }

        public bool SetQueryResult(string queryId, IQueryResult queryResult)
        {
            var target = queryResult as QueryResult;
            if (target != null)
            {
                QueryTaskCompletionSource queryTaskCompletionSource;
                if (_queryTaskDict.TryRemove(queryId, out queryTaskCompletionSource))
                {
                    return queryTaskCompletionSource.TrySetResult(target);
                }
            }

            return false;
        }

        #endregion
    }
}
