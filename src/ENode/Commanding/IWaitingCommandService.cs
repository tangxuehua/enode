using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command service.
    /// </summary>
    public interface IWaitingCommandService
    {
        /// <summary>Try to send an available waiting command to the waiting command queue.
        /// </summary>
        /// <param name="aggregateRootId">The aggregate root id.</param>
        void SendWaitingCommand(object aggregateRootId);
    }
}
