using System;
using System.Collections.Concurrent;
using System.Threading;
using ECommon.Logging;

namespace ENode.Commanding
{
    public class CommandMailbox
    {
        private readonly ConcurrentQueue<ProcessingCommand> _commandQueue;
        private readonly ICommandScheduler _commandScheduler;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILogger _logger;
        private int _isRunning;

        public CommandMailbox(ICommandScheduler commandScheduler, ICommandExecutor commandExecutor, ILoggerFactory loggerFactory)
        {
            _commandQueue = new ConcurrentQueue<ProcessingCommand>();
            _commandScheduler = commandScheduler;
            _commandExecutor = commandExecutor;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public void EnqueueCommand(ProcessingCommand command)
        {
            command.SetMailbox(this);
            _commandQueue.Enqueue(command);
        }
        public bool MarkAsRunning()
        {
            return Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0;
        }
        public void MarkAsNotRunning()
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
        public void CompleteCommand(ProcessingCommand processingCommand)
        {
            _logger.DebugFormat("Command execution completed. cmdType:{0}, cmdId:{1}, aggId:{2}",
                processingCommand.Command.GetType().Name,
                processingCommand.Command.Id,
                processingCommand.AggregateRootId);
            MarkAsNotRunning();
            RegisterForExecution();
        }
        public void RegisterForExecution()
        {
            _commandScheduler.ScheduleCommandMailbox(this);
        }
        public void Run()
        {
            ProcessingCommand currentCommand = null;
            try
            {
                if (_commandQueue.TryDequeue(out currentCommand))
                {
                    _logger.DebugFormat("Start to execute command. cmdType:{0}, cmdId:{1}, aggId:{2}",
                        currentCommand.Command.GetType().Name,
                        currentCommand.Command.Id,
                        currentCommand.AggregateRootId);
                    ExecuteCommand(currentCommand);
                }
            }
            finally
            {
                if (currentCommand == null)
                {
                    MarkAsNotRunning();
                    if (!_commandQueue.IsEmpty)
                    {
                        RegisterForExecution();
                    }
                }
            }
        }

        private void ExecuteCommand(ProcessingCommand command)
        {
            try
            {
                _commandExecutor.ExecuteCommand(command);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Unknown exception caught when executing command. commandType:{0}, commandId:{1}", command.Command.GetType().Name, command.Command.Id), ex);
            }
        }
    }
}
