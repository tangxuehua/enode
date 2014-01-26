namespace ENode.Commanding
{
    /// <summary>Represents a command executor interface.
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>Executes the given command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The context when executing the command.</param>
        void Execute(ICommand command, ICommandExecuteContext context);
    }
}
