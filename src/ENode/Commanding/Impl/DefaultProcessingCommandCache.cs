using System;
using System.Collections.Concurrent;
using ECommon.Extensions;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IProcessingCommandCache.
    /// </summary>
    public class DefaultProcessingCommandCache : IProcessingCommandCache
    {
        private readonly ConcurrentDictionary<Guid, ProcessingCommand> _processingCommandDict = new ConcurrentDictionary<Guid, ProcessingCommand>();

        /// <summary>Add a processing command to memory cache.
        /// </summary>
        /// <param name="processingCommand"></param>
        public void Add(ProcessingCommand processingCommand)
        {
            _processingCommandDict.TryAdd(processingCommand.Command.Id, processingCommand);
        }
        /// <summary>Remove a processing command from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        public void Remove(Guid commandId)
        {
            _processingCommandDict.Remove(commandId);
        }
        /// <summary>Try to get a processing command from memory cache if exists.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        public ProcessingCommand Get(Guid commandId)
        {
            ProcessingCommand processingCommand;
            return _processingCommandDict.TryGetValue(commandId, out processingCommand) ? processingCommand : null;
        }
    }
}
