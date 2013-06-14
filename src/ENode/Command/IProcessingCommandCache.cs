using System;

namespace ENode.Commanding
{
    /// <summary>Represents a memory cache for caching all the processing command info.
    /// </summary>
    public interface IProcessingCommandCache
    {
        /// <summary>Add command into memory cache.
        /// </summary>
        /// <param name="command"></param>
        void Add(ICommand command);
        /// <summary>Try to remove commandInfo from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        void TryRemove(Guid commandId);
        /// <summary>Get commandInfo from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        CommandInfo Get(Guid commandId);
    }
}
