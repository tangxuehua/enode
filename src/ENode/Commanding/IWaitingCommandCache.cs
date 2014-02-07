using System;

namespace ENode.Commanding
{
    /// <summary>Represents a memory cache to store all the commands which are waiting for processing.
    /// </summary>
    public interface IWaitingCommandCache
    {
        /// <summary>Try to add a waiting command for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="processingCommand"></param>
        bool AddWaitingCommand(string aggregateRootId, ProcessingCommand processingCommand);
        /// <summary>Try to fetch a waiting command for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        ProcessingCommand FetchWaitingCommand(string aggregateRootId);
    }
}
