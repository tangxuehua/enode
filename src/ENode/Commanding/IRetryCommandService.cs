namespace ENode.Commanding
{
    /// <summary>Represents a command retry service.
    /// </summary>
    public interface IRetryCommandService
    {
        /// <summary>Retry the given command.
        /// </summary>
        void RetryCommand(ProcessingCommand processingCommand);
        /// <summary>Start the retry command service.
        /// </summary>
        void Start();
    }
}
