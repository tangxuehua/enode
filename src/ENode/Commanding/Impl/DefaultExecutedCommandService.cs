namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IExecutedCommandService.
    /// </summary>
    public class DefaultExecutedCommandService : IExecutedCommandService
    {
        private IWaitingCommandService _waitingCommandService;

        public DefaultExecutedCommandService(IWaitingCommandService waitingCommandService)
        {
            _waitingCommandService = waitingCommandService;
        }

        public void ProcessExecutedCommand(ICommandExecuteContext context, IAggregateCommand command, CommandStatus commandStatus, string processId, string aggregateRootId, string exceptionTypeName, string errorMessage)
        {
            _waitingCommandService.NotifyCommandExecuted(aggregateRootId);
            context.OnCommandExecuted(command, commandStatus, processId, aggregateRootId, exceptionTypeName, errorMessage);
        }
        public void ProcessExecutedCommand(ICommandExecuteContext context, ICommand command, CommandStatus commandStatus, string processId, string exceptionTypeName, string errorMessage)
        {
            context.OnCommandExecuted(command, commandStatus, processId, null, exceptionTypeName, errorMessage);
        }
    }
}
