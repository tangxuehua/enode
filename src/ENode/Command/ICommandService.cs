using System;

namespace ENode.Commanding {
    /// <summary>Represents a service to send or execute command.
    /// </summary>
    public interface ICommandService {
        /// <summary>Send the given command to command queue and return immediately, the command will be handle asynchronously.
        /// <remarks>
        /// Once the command was handled, the callback method will be called.
        /// </remarks>
        /// </summary>
        void Send(ICommand command, Action<CommandAsyncResult> callback = null);
        /// <summary>Send the given command to command queue, and block the current thread until the command was handled or timeout.
        /// </summary>
        void Execute(ICommand command);
    }
}
