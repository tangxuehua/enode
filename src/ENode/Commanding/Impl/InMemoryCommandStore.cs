using System.Collections.Concurrent;
using ECommon.Extensions;

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
    }
}
