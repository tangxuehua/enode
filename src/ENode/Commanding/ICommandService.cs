using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a command service.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>Send a command to execute asynchronously.
        /// </summary>
        /// <param name="command"></param>
        void Send(ICommand command);
        /// <summary>Execute a command asynchronously.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>Returns a task which contains the result of the command.</returns>
        Task<CommandResult> Execute(ICommand command);
        /// <summary>Start a business process (saga).
        /// </summary>
        /// <param name="command">The command to start the process.</param>
        /// <returns>Returns a task which contains the result of the process.</returns>
        Task<ProcessResult> StartProcess(IProcessCommand command);
    }
}
