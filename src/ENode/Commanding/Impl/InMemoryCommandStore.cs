using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.Extensions;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class InMemoryCommandStore : ICommandStore
    {
        private readonly ConcurrentDictionary<string, HandledCommand> _handledCommandDict = new ConcurrentDictionary<string, HandledCommand>();

        public CommandAddResult Add(HandledCommand handledCommand)
        {
            if (_handledCommandDict.TryAdd(handledCommand.Command.Id, handledCommand))
            {
                return CommandAddResult.Success;
            }
            return CommandAddResult.DuplicateCommand;
        }
        public void Remove(string commandId)
        {
            _handledCommandDict.Remove(commandId);
        }
        public HandledCommand Get(string commandId)
        {
            HandledCommand handledCommand;
            if (_handledCommandDict.TryGetValue(commandId, out handledCommand))
            {
                return handledCommand;
            }
            return null;
        }

        public Task<AsyncOperationResult<CommandAddResult>> AddAsync(HandledCommand handledCommand)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<CommandAddResult>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<CommandAddResult>(AsyncOperationResultStatus.Success, null, Add(handledCommand)));
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult> RemoveAsync(string commandId)
        {
            Remove(commandId);
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult>();
            taskCompletionSource.SetResult(AsyncOperationResult.Success);
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult<HandledCommand>> GetAsync(string commandId)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<HandledCommand>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<HandledCommand>(AsyncOperationResultStatus.Success, null, Get(commandId)));
            return taskCompletionSource.Task;
        }
    }
}
