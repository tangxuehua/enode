using System;

namespace ENode.Commanding
{
    /// <summary>Represents a service to process command synchronizely or asyncronizely.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send the given command to command queue and return immediately, the command will be handle asynchronously.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="callback">The callback method when the command was handled.</param>
        void Send(ICommand command, Action<CommandAsyncResult> callback = null);
        /// <summary>Send the given command to command queue, and block the current thread until the command was handled or timeout.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The command execute result.</returns>
        CommandAsyncResult Execute(ICommand command);
    }
}
