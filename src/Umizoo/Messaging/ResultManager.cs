// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging
{
    public class ResultManager : IResultManager
    {
        private readonly ConcurrentDictionary<string, CommandTaskCompletionSource> _commandTaskDict;
        private readonly ConcurrentDictionary<string, QueryTaskCompletionSource> _queryTaskDict;


        public ResultManager()
        {
            _commandTaskDict = new ConcurrentDictionary<string, CommandTaskCompletionSource>();
            _queryTaskDict = new ConcurrentDictionary<string, QueryTaskCompletionSource>();
        }

        public Task<ICommandResult> RegisterProcessingCommand(string commandId, ICommand command,
            CommandReturnMode commandReturnMode, int timeoutMs)
        {
            var taskCompletionSource =
                new CommandTaskCompletionSource(commandReturnMode, timeoutMs, commandId, _commandTaskDict.Remove);
            if (!_commandTaskDict.TryAdd(commandId, taskCompletionSource))
            {
                taskCompletionSource.Dispose();
                throw new ApplicationException(
                    string.Format("Duplicate processing command registration, type:{0}, id:{1}", command.GetType().Name,
                        commandId));
            }

            return taskCompletionSource.Task;
        }

        public Task<IQueryResult> RegisterProcessingQuery(string queryId, IQuery query, int timeoutMs)
        {
            var taskCompletionSource = new QueryTaskCompletionSource(timeoutMs, queryId, _queryTaskDict.Remove);
            if (!_queryTaskDict.TryAdd(queryId, taskCompletionSource))
            {
                taskCompletionSource.Dispose();
                throw new ApplicationException(
                    string.Format("Duplicate processing query registration, type:{0}, id:{1}", query.GetType().Name,
                        queryId));
            }

            return taskCompletionSource.Task;
        }

        public int WaitingCommands => _commandTaskDict.Count;

        public int WaitingQueries => _queryTaskDict.Count;

        private abstract class WrappedTaskCompletionSource<T1, T2> : DisposableObject
            where T2 : class, T1
        {
            private readonly Action<string> callback;
            private readonly SingleEntryGate setResultGate;

            private readonly string sourceId;
            private readonly TaskCompletionSource<T1> taskCompletionSource;
            private readonly Timer timer;

            public WrappedTaskCompletionSource(int timeoutMs, string sourceId, Action<string> callback)
            {
                taskCompletionSource = new TaskCompletionSource<T1>();
                setResultGate = new SingleEntryGate();
                this.sourceId = sourceId;
                this.callback = callback;
                if (timeoutMs > 0) timer = new Timer(HandleTimeout, null, timeoutMs, Timeout.Infinite);
            }

            protected abstract T1 TimeoutResult { get; }

            public Task<T1> Task => taskCompletionSource.Task;

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (timer != null)
                        timer.Dispose();
                    taskCompletionSource.TrySetCanceled();
                }
            }

            private void HandleTimeout(object state)
            {
                if (!setResultGate.TryEnter()) return;

                taskCompletionSource.SetResult(TimeoutResult);

                if (callback != null) callback(sourceId);

                timer.Dispose();
            }

            protected virtual bool CanSetResult(T2 result)
            {
                return true;
            }

            public bool TrySetResult(T2 result)
            {
                if (!CanSetResult(result))
                    return false;

                if (!setResultGate.TryEnter()) return false;

                if (callback != null) callback(sourceId);
                if (timer != null) timer.Dispose();

                return taskCompletionSource.TrySetResult(result);
            }
        }

        private class QueryTaskCompletionSource : WrappedTaskCompletionSource<IQueryResult, QueryResult>
        {
            public QueryTaskCompletionSource(int timeoutMs, string queryId, Action<string> callback)
                : base(timeoutMs, queryId, callback)
            {
            }

            protected override IQueryResult TimeoutResult {get { return QueryResult.Timeout; } }
        }

        private class CommandTaskCompletionSource : WrappedTaskCompletionSource<ICommandResult, CommandResult>
        {
            public CommandTaskCompletionSource(CommandReturnMode returnMode, int timeoutMs, string commandId,
                Action<string> callback)
                : base(timeoutMs, commandId, callback)
            {
                ReturnMode = returnMode;
            }

            protected override ICommandResult TimeoutResult { get { return CommandResult.Timeout; } }

            public CommandReturnMode ReturnMode { get; }

            public int EventCount { get; set; }

            protected override bool CanSetResult(CommandResult result)
            {
                if (result.Status != HandleStatus.Success || result.ReplyType == CommandReturnMode.Manual) return true;

                var completed = false;
                switch (result.ReplyType)
                {
                    case CommandReturnMode.CommandExecuted:
                        if (ReturnMode == CommandReturnMode.CommandExecuted) completed = true;
                        else EventCount = result.ProducedEventCount;
                        break;
                    case CommandReturnMode.EventHandled:
                        completed = --EventCount <= 0;
                        break;
                    case CommandReturnMode.Delivered:
                        completed = ReturnMode == CommandReturnMode.Delivered;
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
                    return commandTaskCompletionSource.TrySetResult(target);
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
                    return queryTaskCompletionSource.TrySetResult(target);
            }

            return false;
        }

        #endregion
    }
}