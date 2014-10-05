using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;

namespace ENode.Commanding.Impl
{
    public class DefaultWaitingCommandService : IWaitingCommandService
    {
        private readonly object _lockObject = new object();
        private readonly IDictionary<string, int> _commandCountDict = new Dictionary<string, int>();
        private readonly IDictionary<string, Queue<ProcessingCommand>> _commandQueueDict = new Dictionary<string, Queue<ProcessingCommand>>();
        private readonly BlockingCollection<ProcessingCommand> _queue;
        private readonly ILogger _logger;
        private readonly Worker _worker;
        private readonly IActionExecutionService _actionExecutionService;
        private ICommandExecutor _commandExecutor;

        public DefaultWaitingCommandService(IActionExecutionService actionExecutionService, ILoggerFactory loggerFactory)
        {
            _queue = new BlockingCollection<ProcessingCommand>(new ConcurrentQueue<ProcessingCommand>());
            _worker = new Worker("ExecuteWaitingCommand", () => _commandExecutor.Execute(_queue.Take()));
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }
        public void Start()
        {
            _worker.Start();
        }
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
