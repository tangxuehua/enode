
namespace ENode.Commanding
{
    /// <summary>Represents a service which process the executed command.
    /// </summary>
    public interface IExecutedCommandService
    {
        /// <summary>Process the executed aggregate command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="commandStatus"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="exceptionTypeName"></param>
        /// <param name="errorMessage"></param>
        void ProcessExecutedCommand(ICommandExecuteContext context, IAggregateCommand command, CommandStatus commandStatus, string aggregateRootId, string exceptionTypeName, string errorMessage);
        /// <summary>Process the executed command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="commandStatus"></param>
        /// <param name="exceptionTypeName"></param>
        /// <param name="errorMessage"></param>
        void ProcessExecutedCommand(ICommandExecuteContext context, ICommand command, CommandStatus commandStatus, string exceptionTypeName, string errorMessage);
    }
}
