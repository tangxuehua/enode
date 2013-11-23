using System;
using System.Threading.Tasks;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents mapping service which mapping which domain events will result in the command completed.
    /// </summary>
    public interface ICommandTaskManager
    {
        /// <summary>Creates a new command task by the given commandId.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        Task<CommandResult> CreateCommandTask(Guid commandId);
        /// <summary>Completes a command task.
        /// </summary>
        /// <param name="commandId"></param>
        void CompleteCommandTask(Guid commandId);
        /// <summary>Completes a command task.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="errorMessage"></param>
        void CompleteCommandTask(Guid commandId, string errorMessage);
        /// <summary>Completes a command task.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="exception"></param>
        void CompleteCommandTask(Guid commandId, Exception exception);
        /// <summary>Completes a command task.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="exception"></param>
        void CompleteCommandTask(Guid commandId, string errorMessage, Exception exception);
    }
}
