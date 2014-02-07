namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IWaitingCommandService.
    /// </summary>
    public class DefaultWaitingCommandService : IWaitingCommandService
    {
        private readonly IWaitingCommandCache _waitingCommandCache;
        private readonly ProcessingCommandProcessor _processor;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="waitingCommandCache"></param>
        /// <param name="commandExecutor"></param>
        public DefaultWaitingCommandService(IWaitingCommandCache waitingCommandCache)
        {
            _waitingCommandCache = waitingCommandCache;
            _processor = new ProcessingCommandProcessor();
        }

        /// <summary>Set the command executor.
        /// </summary>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _processor.SetCommandExecutor(commandExecutor);
        }
        /// <summary>Start the waiting command service.
        /// </summary>
        public void Start()
        {
            _processor.Start();
        }

        /// <summary>Try to send an available waiting command to the waiting command queue.
        /// </summary>
        /// <param name="aggregateRootId">The aggregate root id.</param>
        public void SendWaitingCommand(string aggregateRootId)
        {
            var processingCommand = _waitingCommandCache.FetchWaitingCommand(aggregateRootId);
            if (processingCommand != null)
            {
                processingCommand.CommandExecuteContext.CheckCommandWaiting = false;
                _processor.AddProcessingCommand(processingCommand);
            }
        }
    }
}
