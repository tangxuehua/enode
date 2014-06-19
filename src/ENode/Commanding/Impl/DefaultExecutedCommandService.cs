namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IExecutedCommandProcessService.
    /// </summary>
    public class DefaultExecutedCommandService : IExecutedCommandService
    {
        private IWaitingCommandService _waitingCommandService;

        /// <summary>Parameteriazed constructor.
        /// </summary>
        /// <param name="waitingCommandService"></param>
        public DefaultExecutedCommandService(IWaitingCommandService waitingCommandService)
        {
            _waitingCommandService = waitingCommandService;
        }

        /// <summary>Process a command which has been executed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="commandStatus"></param>
        /// <param name="processId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="exceptionTypeName"></param>
        /// <param name="errorMessage"></param>
        public void ProcessExecutedCommand(ICommandExecuteContext context, ICommand command, CommandStatus commandStatus, string processId, string aggregateRootId, string exceptionTypeName, string errorMessage)
        {
            _waitingCommandService.NotifyCommandExecuted(aggregateRootId);
            context.OnCommandExecuted(command, commandStatus, processId, aggregateRootId, exceptionTypeName, errorMessage);
        }
    }
}
