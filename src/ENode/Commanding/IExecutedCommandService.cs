using System;

namespace ENode.Commanding
{
    /// <summary>Represents a service which process the executed command.
    /// </summary>
    public interface IExecutedCommandService
    {
        /// <summary>Process a command which has been executed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="commandStatus"></param>
        /// <param name="processId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="exceptionTypeName"></param>
        /// <param name="errorMessage"></param>
        void ProcessExecutedCommand(ICommandExecuteContext context, ICommand command, CommandStatus commandStatus, string processId, string aggregateRootId, string exceptionTypeName, string errorMessage);
    }
}
