using System;
using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a command service.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send the command to a specific command queue and returns a task object.
        /// </summary>
        /// <param name="command">The command to send.</param>
        Task<CommandResult> Send(ICommand command);
    }
}
