using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class DefaultRetryCommandService : IRetryCommandService
    {
        private ICommandAsyncResultManager _commandAsyncResultManager;
        private ICommandQueue _retryCommandQueue;
        private ILogger _logger;

        public DefaultRetryCommandService(ICommandAsyncResultManager commandAsyncResultManager, ILoggerFactory loggerFactory)
        {
            _commandAsyncResultManager = commandAsyncResultManager;
            _logger = loggerFactory.Create(GetType().Name);
        }

        public void RetryCommand(CommandInfo commandInfo, Exception exception)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = Configuration.Instance.GetRetryCommandQueue();
            }

            if (commandInfo.RetriedCount < commandInfo.Command.RetryCount)
            {
                _retryCommandQueue.Enqueue(commandInfo.Command);
                commandInfo.IncreaseRetriedCount();
                _logger.InfoFormat("Sent {0} to retry queue for {1} time.", commandInfo.Command.GetType().Name, commandInfo.RetriedCount);
            }
            else
            {
                _commandAsyncResultManager.TryComplete(commandInfo.Command.Id, exception);
            }
        }
    }
}
