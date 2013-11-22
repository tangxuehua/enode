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
        /// <param name="command"></param>
        bool AddWaitingCommand(object aggregateRootId, ICommand command);
        /// <summary>Try to fetch a waiting command for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        ICommand FetchWaitingCommand(object aggregateRootId);
    }
}
