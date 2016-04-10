using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Commanding
{
    /// <summary>Represents a command service.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send a command asynchronously.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>A task which contains the send result of the command.</returns>
        Task<AsyncTaskResult> SendAsync(ICommand command);
        /// <summary>Send a command synchronously.
        /// </summary>
        /// <param name="command">The command to send.</param>
        void Send(ICommand command);
        /// <summary>Execute a command synchronously with the default command return type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeoutMillis"></param>
        /// <returns></returns>
        CommandResult Execute(ICommand command, int timeoutMillis);
        /// <summary>Execute a command synchronously with the specified command return type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandReturnType"></param>
        /// <param name="timeoutMillis"></param>
        /// <returns></returns>
        CommandResult Execute(ICommand command, CommandReturnType commandReturnType, int timeoutMillis);
        /// <summary>Execute a command asynchronously with the default command return type.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A task which contains the result of the command.</returns>
        Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command);
        /// <summary>Execute a command asynchronously with the specified command return type.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandReturnType">The return type of the command.</param>
        /// <returns>A task which contains the result of the command.</returns>
        Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command, CommandReturnType commandReturnType);
    }
}
