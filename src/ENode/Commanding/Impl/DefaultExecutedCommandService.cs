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
        /// <param name="aggregateRootId"></param>
        /// <param name="exceptionTypeName"></param>
        /// <param name="errorMessage"></param>
        public void ProcessExecutedCommand(ICommandExecuteContext context, ICommand command, CommandStatus commandStatus, string aggregateRootId, string exceptionTypeName, string errorMessage)
        {
            var currentAggregateRootId = aggregateRootId;
            if (string.IsNullOrEmpty(currentAggregateRootId))
            {
                currentAggregateRootId = command.AggregateRootId;
            }
            _waitingCommandService.NotifyCommandExecuted(currentAggregateRootId);
            context.OnCommandExecuted(command, commandStatus, currentAggregateRootId, exceptionTypeName, errorMessage);
        }
    }
}
