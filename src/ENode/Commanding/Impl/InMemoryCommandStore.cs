using System.Collections.Concurrent;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandStore.
    /// </summary>
    public class InMemoryCommandStore : ICommandStore
    {
        private readonly ConcurrentDictionary<string, HandledCommand> _dict = new ConcurrentDictionary<string, HandledCommand>();

        public CommandAddResult AddCommand(HandledCommand handledCommand)
        {
            if (_dict.TryAdd(handledCommand.Command.Id, handledCommand))
            {
                return CommandAddResult.Success;
            }
            return CommandAddResult.DuplicateCommand;
        }
        public HandledCommand Find(string commandId)
        {
            HandledCommand handledCommand;
            if (_dict.TryGetValue(commandId, out handledCommand))
            {
                return handledCommand;
            }
            return null;
        }
    }
}
