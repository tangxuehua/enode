namespace ENode.Commanding
{
    /// <summary>Represents a command retry service when the command execution has concurrent exception.
    /// </summary>
    public interface IRetryCommandService
    {
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Retry the given command.
        /// </summary>
        void RetryCommand(ProcessingCommand processingCommand);
        /// <summary>Start the retry command service.
        /// </summary>
        void Start();
    }
}
