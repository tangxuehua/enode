using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.Retring;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class InMemoryCommandStore : ICommandStore
    {
        private readonly Task<AsyncTaskResult> _successTask = Task.FromResult(AsyncTaskResult.Success);
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

        public Task<AsyncTaskResult<CommandAddResult>> AddAsync(HandledCommand handledCommand)
        {
            return Task.FromResult<AsyncTaskResult<CommandAddResult>>(new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, Add(handledCommand)));
        }
        public Task<AsyncTaskResult> RemoveAsync(string commandId)
        {
            Remove(commandId);
            return _successTask;
        }
        public Task<AsyncTaskResult<HandledCommand>> GetAsync(string commandId)
        {
            return Task.FromResult<AsyncTaskResult<HandledCommand>>(new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.Success, null, Get(commandId)));
        }
    }
}
