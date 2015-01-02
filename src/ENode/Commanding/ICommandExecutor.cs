namespace ENode.Commanding
{
    /// <summary>Represents a command executor.
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>Executes the given command.
        /// </summary>
        /// <param name="processingCommand">The command to execute.</param>
        void ExecuteCommand(ProcessingCommand processingCommand);
    }
}
