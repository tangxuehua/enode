
namespace ENode.Commanding
{
    /// <summary>Represents a service which manages all the waiting command.
    /// </summary>
    public interface IWaitingCommandService
    {
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Start the service. A worker will be started, which takes command from the processing queue to process.
        /// </summary>
        void Start();
        /// <summary>Register a command.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <returns>Returns true if the given command is added into the aggregate waiting queue; otherwise returns false.</returns>
        bool RegisterCommand(ProcessingCommand processingCommand);
        /// <summary>Notify that a command of the given aggregate has been executed, and the next command will be execute if exist.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        void NotifyCommandExecuted(string aggregateRootId);
    }
}
