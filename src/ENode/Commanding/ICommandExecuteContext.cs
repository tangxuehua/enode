
namespace ENode.Commanding
{
    /// <summary>Represents a context environment for command executor executing command.
    /// </summary>
    public interface ICommandExecuteContext : ICommandContext, ITrackingContext
    {
        /// <summary>Notify the given command is executed.
        /// </summary>
        void OnCommandExecuted(CommandResult commandResult);
    }
}
