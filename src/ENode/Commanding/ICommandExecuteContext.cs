using System.Collections.Generic;

namespace ENode.Commanding
{
    /// <summary>Represents a context environment for command executor executing command.
    /// </summary>
    public interface ICommandExecuteContext : ICommandContext, ITrackingContext
    {
        /// <summary>A dictionary contains some additional information of the current command execution context.
        /// </summary>
        IDictionary<string, string> Items { get; }
        /// <summary>Check whether need to apply the command waiting logic when the command is executing.
        /// </summary>
        bool CheckCommandWaiting { get; set; }
        /// <summary>Notify the given command is executed.
        /// </summary>
        void OnCommandExecuted(ICommand command, CommandStatus commandStatus, string processId, string aggregateRootId, string exceptionTypeName, string errorMessage);
    }
}
