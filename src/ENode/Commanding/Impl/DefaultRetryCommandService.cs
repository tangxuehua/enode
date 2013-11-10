using System;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IRetryCommandService.
    /// </summary>
    public class DefaultRetryCommandService : IRetryCommandService
    {
        private ICommandQueue _retryCommandQueue;
        private readonly ILogger _logger;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="commandAsyncResultManager"></param>
        /// <param name="loggerFactory"></param>
        public DefaultRetryCommandService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
        }

        /// <summary>Retry the given command.
        /// </summary>
        /// <param name="commandInfo"></param>
        /// <param name="eventStream"></param>
        /// <param name="errorInfo"></param>
        public void RetryCommand(CommandInfo commandInfo, EventStream eventStream, ErrorInfo errorInfo)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = Configuration.Instance.GetRetryCommandQueue();
            }
            var command = commandInfo.Command;

            if (commandInfo.RetriedCount < command.RetryCount)
            {
                _retryCommandQueue.Enqueue(commandInfo.Command);
                commandInfo.IncreaseRetriedCount();
            }
            else
            {
                _logger.ErrorFormat("{0} retried count reached to its max retry count {1}.", command.GetType().Name, command.RetryCount);
            }
        }
    }
}
