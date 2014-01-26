using System;

namespace ENode.Commanding
{
    /// <summary>Represents a memory cache for caching all the processing commands.
    /// </summary>
    public interface IProcessingCommandCache
    {
        /// <summary>Add a processing command to memory cache.
        /// </summary>
        /// <param name="processingCommand"></param>
        void Add(ProcessingCommand processingCommand);
        /// <summary>Remove a processing command from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        void Remove(Guid commandId);
        /// <summary>Try to get a processing command from memory cache if exists.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        ProcessingCommand Get(Guid commandId);
    }
}
