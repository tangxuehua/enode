using System.Collections.Concurrent;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IRetryCommandService.
    /// </summary>
    public class DefaultRetryCommandService : IRetryCommandService
    {
        private readonly BlockingCollection<ProcessingCommand> _queue;
        private readonly Worker _worker;
        private readonly ILogger _logger;
        private readonly IActionExecutionService _actionExecutionService;
        private ICommandExecutor _commandExecutor;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultRetryCommandService(IActionExecutionService actionExecutionService, ILoggerFactory loggerFactory)
        {
            _queue = new BlockingCollection<ProcessingCommand>(new ConcurrentQueue<ProcessingCommand>());
            _worker = new Worker("ExecuteRetringCommand", () => _commandExecutor.Execute(_queue.Take()));
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }
        /// <summary>Start the service.
        /// </summary>
        public void Start()
        {
            _worker.Start();
        }

        public bool RetryConcurrentCommand(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;

            if (processingCommand.RetriedCount < command.RetryCount)
            {
                processingCommand.IncreaseRetriedCount();
                RetryCommand(processingCommand);
                _logger.DebugFormat("{0} [id:{1}, aggregateId:{2}] was added into the retry queue, current retry count:{3}.", command.GetType().Name, command.Id, processingCommand.AggregateRootId, processingCommand.RetriedCount);
                return true;
            }
            else
            {
                _logger.DebugFormat("{0} [id:{1}, aggregateId:{2}] retried count reached to its max retry count {3}.", command.GetType().Name, command.Id, processingCommand.AggregateRootId, command.RetryCount);
                return false;
            }
        }
        public void RetryCommand(ProcessingCommand processingCommand)
        {
            processingCommand.CommandExecuteContext.Clear();
            processingCommand.CommandExecuteContext.CheckCommandWaiting = false;
            _queue.Add(processingCommand);
        }
    }
}
