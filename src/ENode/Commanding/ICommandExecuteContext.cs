using System;
using System.Collections.Generic;
namespace ENode.Commanding
{
    /// <summary>Represents a context environment for command executor executing command.
    /// </summary>
    public interface ICommandExecuteContext : ICommandContext, ITrackingContext
    {
        /// <summary>A dictionary contains some additional information of the current command execution context.
        /// </summary>
        IDictionary<string, object> Items { get; }
        /// <summary>Check whether need to apply the command waiting logic when the command is executing.
        /// </summary>
        bool CheckCommandWaiting { get; set; }
        /// <summary>Notify the given command execution is failed.
        /// </summary>
        void OnCommandExecuted(ICommand command, string errorMessage);
    }
}
