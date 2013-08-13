using System;

namespace ENode.Commanding
{
    /// <summary>Represents a memory cache for caching all the processing commands.
    /// </summary>
    public interface IProcessingCommandCache
    {
        /// <summary>Add a command into memory cache.
        /// </summary>
        /// <param name="command"></param>
        void Add(ICommand command);
        /// <summary>Try to remove a command from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        void TryRemove(Guid commandId);
        /// <summary>Get the command info from memory cache.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        CommandInfo Get(Guid commandId);
    }
}
