namespace ENode.Commanding
{
    /// <summary>Represents a context environment for command executor executing command.
    /// </summary>
    public interface ICommandExecuteContext : ICommandContext, ITrackingContext
    {
        /// <summary>Notify the given command has been executed.
        /// </summary>
        /// <param name="command">The executed command.</param>
        void OnCommandExecuted(ICommand command);
    }
}
