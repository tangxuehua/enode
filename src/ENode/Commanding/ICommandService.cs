using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command service.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send the command to a specific command queue.
        /// </summary>
        /// <param name="command">The command to send.</param>
        void Send(ICommand command);
    }
}
