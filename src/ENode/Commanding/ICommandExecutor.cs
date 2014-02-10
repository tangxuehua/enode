namespace ENode.Commanding
{
    /// <summary>Represents a command executor interface.
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>Executes the given command.
        /// </summary>
        /// <param name="processingCommand">The command to execute.</param>
        void Execute(ProcessingCommand processingCommand);
    }
}
