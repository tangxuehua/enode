using ECommon.Logging;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IRetryCommandService.
    /// </summary>
    public class DefaultRetryCommandService : IRetryCommandService
    {
        private readonly ProcessingCommandProcessor _processor;
        private readonly ILogger _logger;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultRetryCommandService(ILoggerFactory loggerFactory)
        {
            _processor = new ProcessingCommandProcessor();
            _logger = loggerFactory.Create(GetType().Name);
        }

        /// <summary>Set the command executor.
        /// </summary>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _processor.SetCommandExecutor(commandExecutor);
        }
        /// <summary>Start the retry command service.
        /// </summary>
        public void Start()
        {
            _processor.Start();
        }

        /// <summary>Retry the given processing command.
        /// </summary>
        /// <param name="processingCommand"></param>
        public void RetryCommand(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            if (processingCommand.RetriedCount < command.RetryCount)
            {
                processingCommand.IncreaseRetriedCount();
                _processor.AddProcessingCommand(processingCommand);
            }
            else
            {
                _logger.InfoFormat("{0} retried count reached to its max retry count {1}.", command.GetType().Name, command.RetryCount);
            }
        }
    }
}
