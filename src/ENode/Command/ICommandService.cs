using System;

namespace ENode.Commanding
{
    /// <summary>Represents a service to send or execute command.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send a command asynchronously.
        /// </summary>
        void Send(ICommand command, Action<CommandAsyncResult> callback = null);
        /// <summary>Execute a command synchronously.
        /// </summary>
        void Execute(ICommand command);
    }
}
