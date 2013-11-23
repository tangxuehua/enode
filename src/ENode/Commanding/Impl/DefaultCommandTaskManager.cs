using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of command task manager interface.
    /// </summary>
    public class DefaultCommandTaskManager : ICommandTaskManager
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<CommandResult>> _commandTaskDict = new ConcurrentDictionary<Guid, TaskCompletionSource<CommandResult>>();

        public Task<CommandResult> CreateCommandTask(Guid commandId)
        {
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();
            if (!_commandTaskDict.TryAdd(commandId, taskCompletionSource))
            {
                throw new Exception(string.Format("Duplicated command with id:{0}.", commandId));
            }
            return taskCompletionSource.Task;
        }

        public void CompleteCommandTask(Guid commandId)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryRemove(commandId, out taskCompletionSource))
            {
                taskCompletionSource.SetResult(CommandResult.Success);
            }
        }
        public void CompleteCommandTask(Guid commandId, string errorMessage)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryRemove(commandId, out taskCompletionSource))
            {
                taskCompletionSource.SetResult(new CommandResult(errorMessage));
            }
        }
        public void CompleteCommandTask(Guid commandId, Exception exception)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryRemove(commandId, out taskCompletionSource))
            {
                taskCompletionSource.SetResult(new CommandResult(exception));
            }
        }
        public void CompleteCommandTask(Guid commandId, string errorMessage, Exception exception)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryRemove(commandId, out taskCompletionSource))
            {
                taskCompletionSource.SetResult(new CommandResult(errorMessage, exception));
            }
        }
    }
}
