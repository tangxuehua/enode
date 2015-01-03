using System;
using System.Collections.Concurrent;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;

namespace ENode.Commanding
{
    public class CommandMailbox
    {
        private readonly ConcurrentQueue<ProcessingCommand> _commandQueue;
        private readonly ICommandDispatcher _dispatcher;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILogger _logger;
        private int _isRunning;

        public CommandMailbox(ICommandDispatcher dispatcher, ICommandExecutor commandExecutor, ILoggerFactory loggerFactory)
        {
            _commandQueue = new ConcurrentQueue<ProcessingCommand>();
            _dispatcher = dispatcher;
            _commandExecutor = commandExecutor;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public void EnqueueCommand(ProcessingCommand command)
        {
            _commandQueue.Enqueue(command);
        }
        public bool MarkAsRunning()
        {
            return Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0;
        }
        public void Run()
        {
            var executedCommandCount = 0;
            var deadline = DateTime.Now.Ticks;
            try
            {
                ProcessingCommand currentCommand;
                while (_commandQueue.TryDequeue(out currentCommand))
                {
                    ExecuteCommand(currentCommand);
                    executedCommandCount++;
                    if (ShouldStop(deadline, executedCommandCount))
                    {
                        break;
                    }
                }
            }
            finally
            {
                MarkAsNotRunning();
                if (executedCommandCount > 0)
                {
                    _dispatcher.RegisterMailboxForExecution(this);
                }
                else
                {
                    _dispatcher.RegisterMailboxForDelayExecution(this, 1000);
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
                _logger.Error("Unknown exception caught when executing command.", ex);
            }
        }
        private bool ShouldStop(long deadline, int executedCommandCount)
        {
            return executedCommandCount >= _dispatcher.ExecuteCommandCountOfOneTask
                || (_dispatcher.TaskMaxDeadlineMilliseconds > 0 && ((DateTime.Now.Ticks - deadline) / 10000) >= _dispatcher.TaskMaxDeadlineMilliseconds);
        }
        private void MarkAsNotRunning()
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }
}
