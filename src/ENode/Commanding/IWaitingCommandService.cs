using System;

namespace ENode.Commanding
{
    /// <summary>Represents a service to send waiting commands to waiting command queue.
    /// </summary>
    public interface IWaitingCommandService
    {
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Try to send an available waiting command to the waiting command queue.
        /// </summary>
        /// <param name="aggregateRootId">The aggregate root id.</param>
        void SendWaitingCommand(string aggregateRootId);
        /// <summary>Start the waiting command service.
        /// </summary>
        void Start();
    }
}
