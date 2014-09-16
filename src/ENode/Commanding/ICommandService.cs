using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a command service.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send a command synchronously.
        /// </summary>
        /// <param name="command">The command to send.</param>
        void Send(ICommand command);
        /// <summary>Send a command asynchronously.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>A task which contains the send result of the command.</returns>
        Task<CommandSendResult> SendAsync(ICommand command);
        /// <summary>Execute a command asynchronously with the default command return type.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A task which contains the result of the command.</returns>
        Task<CommandResult> Execute(ICommand command);
        /// <summary>Execute a command asynchronously with the specified command return type.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandReturnType">The return type of the command.</param>
        /// <returns>A task which contains the result of the command.</returns>
        Task<CommandResult> Execute(ICommand command, CommandReturnType commandReturnType);
        /// <summary>Start a business process (saga), and returns a task.
        /// <remarks>
        /// The task will return when the process is completed successfully or any command associated with the process has error or exception.
        /// </remarks>
        /// </summary>
        /// <param name="command">The command which starts the process.</param>
        /// <returns>A task which contains the result of the process.</returns>
        Task<ProcessResult> StartProcess(IStartProcessCommand command);
    }
}
