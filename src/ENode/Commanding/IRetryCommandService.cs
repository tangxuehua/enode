namespace ENode.Commanding
{
    /// <summary>Represents a command retry service to retry the command if it has concurrent exception.
    /// </summary>
    public interface IRetryCommandService
    {
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Retry the given command.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <returns>Returns true if the given command was added into the retry queue; otherwise, returns false.</returns>
        bool RetryCommand(ProcessingCommand processingCommand);
        /// <summary>Start the service.
        /// </summary>
        void Start();
    }
}
