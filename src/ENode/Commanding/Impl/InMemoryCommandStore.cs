using System.Collections.Concurrent;
using ECommon.Extensions;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandStore.
    /// </summary>
    public class InMemoryCommandStore : ICommandStore
    {
        private readonly ConcurrentDictionary<string, HandledCommand> _handledCommandDict = new ConcurrentDictionary<string, HandledCommand>();
        private readonly ConcurrentDictionary<string, HandledAggregateCommand> _dict = new ConcurrentDictionary<string, HandledAggregateCommand>();

        public CommandAddResult AddHandledAggregateCommand(HandledAggregateCommand handledCommand)
        {
            if (_dict.TryAdd(handledCommand.Command.Id, handledCommand))
            {
                return CommandAddResult.Success;
            }
            return CommandAddResult.DuplicateCommand;
        }
        public HandledAggregateCommand FindHandledAggregateCommand(string commandId)
        {
            HandledAggregateCommand handledCommand;
            if (_dict.TryGetValue(commandId, out handledCommand))
            {
                return handledCommand;
            }
            return null;
        }
        public void Remove(string commandId)
        {
            _dict.Remove(commandId);
        }
        public CommandAddResult AddHandledCommand(HandledCommand handledCommand)
        {
            if (_handledCommandDict.TryAdd(handledCommand.Command.Id, handledCommand))
            {
                return CommandAddResult.Success;
            }
            return CommandAddResult.DuplicateCommand;
        }

        public HandledCommand FindHandledCommand(string commandId)
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
