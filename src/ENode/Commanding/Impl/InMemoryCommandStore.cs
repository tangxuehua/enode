using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Commanding.Impl
{
    public class InMemoryCommandStore : ICommandStore
    {
        private readonly ConcurrentDictionary<string, HandledCommand> _handledCommandDict = new ConcurrentDictionary<string, HandledCommand>();

        public Task<AsyncTaskResult<CommandAddResult>> AddAsync(HandledCommand handledCommand)
        {
            return Task.FromResult<AsyncTaskResult<CommandAddResult>>(new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, Add(handledCommand)));
        }
        public Task<AsyncTaskResult<HandledCommand>> GetAsync(string commandId)
        {
            return Task.FromResult<AsyncTaskResult<HandledCommand>>(new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.Success, null, Get(commandId)));
        }

        private CommandAddResult Add(HandledCommand handledCommand)
        {
            if (_handledCommandDict.TryAdd(handledCommand.CommandId, handledCommand))
            {
                return CommandAddResult.Success;
            }
            return CommandAddResult.DuplicateCommand;
        }
        private HandledCommand Get(string commandId)
        {
            HandledCommand handledCommand;
            if (_handledCommandDict.TryGetValue(commandId, out handledCommand))
            {
                return handledCommand;
            }
            return null;
        }
    }
}
