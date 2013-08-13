using System;
using System.Collections.Concurrent;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IProcessingCommandCache.
    /// </summary>
    public class DefaultProcessingCommandCache : IProcessingCommandCache
    {
        private readonly ConcurrentDictionary<Guid, CommandInfo> _commandInfoDict = new ConcurrentDictionary<Guid, CommandInfo>();

        /// <summary>Add a command to memory cache.
        /// </summary>
        /// <param name="command"></param>
        public void Add(ICommand command)
        {
            _commandInfoDict.TryAdd(command.Id, new CommandInfo(command));
        }
        /// <summary>Remove a command from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        public void TryRemove(Guid commandId)
        {
            CommandInfo commandInfo;
            _commandInfoDict.TryRemove(commandId, out commandInfo);
        }
        /// <summary>Try to get the command info from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        public CommandInfo Get(Guid commandId)
        {
            CommandInfo commandInfo;
            return _commandInfoDict.TryGetValue(commandId, out commandInfo) ? commandInfo : null;
        }
    }
}
