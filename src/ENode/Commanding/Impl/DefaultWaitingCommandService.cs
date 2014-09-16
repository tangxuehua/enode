using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.Logging;
using ECommon.Scheduling;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IWaitingCommandService.
    /// </summary>
    public class DefaultWaitingCommandService : IWaitingCommandService
    {
        private readonly object _lockObject = new object();
        private readonly IDictionary<string, int> _commandCountDict = new Dictionary<string, int>();
        private readonly IDictionary<string, Queue<ProcessingCommand>> _commandQueueDict = new Dictionary<string, Queue<ProcessingCommand>>();
        private readonly BlockingCollection<ProcessingCommand> _queue;
        private readonly ILogger _logger;
        private readonly Worker _worker;
        private ICommandExecutor _commandExecutor;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultWaitingCommandService(ILoggerFactory loggerFactory)
        {
            _queue = new BlockingCollection<ProcessingCommand>(new ConcurrentQueue<ProcessingCommand>());
            _worker = new Worker("ExecuteWaitingCommand", () => _commandExecutor.Execute(_queue.Take()));
            _logger = loggerFactory.Create(GetType().FullName);
        }

        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }
        /// <summary>Start the service. A worker will be started, which takes command from the processing queue to process.
        /// </summary>
        public void Start()
        {
            _worker.Start();
        }
        /// <summary>Register a command.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <returns>Returns true if the given command is added into the aggregate waiting queue; otherwise returns false.</returns>
        public bool RegisterCommand(ProcessingCommand processingCommand)
        {
            if (processingCommand.Command is ICreatingAggregateCommand)
            {
                return false;
            }
            if (!(processingCommand.Command is IAggregateCommand))
            {
                return false;
            }
            var aggregateRootId = processingCommand.AggregateRootId;
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (_commandCountDict.ContainsKey(aggregateRootId))
                {
                    _commandCountDict[aggregateRootId] += 1;
                }
                else
                {
                    _commandCountDict.Add(aggregateRootId, 1);
                }

                if (_commandCountDict[aggregateRootId] > 1)
                {
                    if (!_commandQueueDict.ContainsKey(aggregateRootId))
                    {
                        _commandQueueDict.Add(aggregateRootId, new Queue<ProcessingCommand>());
                    }
                    _commandQueueDict[aggregateRootId].Enqueue(processingCommand);
                    _logger.DebugFormat("Queued a waiting command, commandType:{0}, commandId:{1}, aggregateRootId:{2}",
                        processingCommand.Command.GetType().Name,
                        processingCommand.Command.Id,
                        aggregateRootId);
                    return true;
                }

                return false;
            }
        }
        /// <summary>Notify that a command of the given aggregate has been executed, and the next command will be execute if exist.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public void NotifyCommandExecuted(string aggregateRootId)
        {
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                return;
            }
            lock (_lockObject)
            {
                if (_commandCountDict.ContainsKey(aggregateRootId))
                {
                    ProcessingCommand processingCommand = null;
                    _commandCountDict[aggregateRootId] -= 1;

                    if (_commandCountDict[aggregateRootId] > 0)
                    {
                        processingCommand = _commandQueueDict[aggregateRootId].Dequeue();
                    }
                    else
                    {
                        _commandCountDict.Remove(aggregateRootId);
                        _commandQueueDict.Remove(aggregateRootId);
                    }

                    if (processingCommand != null)
                    {
                        processingCommand.CommandExecuteContext.CheckCommandWaiting = false;
                        _queue.Add(processingCommand);
                    }
                }
            }
        }
    }
}
