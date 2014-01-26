using System;
using ECommon.Logging;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;

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
        /// <param name="loggerFactory"></param>
        public DefaultRetryCommandService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
        }

        /// <summary>Retry the given command.
        /// </summary>
        /// <param name="commandInfo"></param>
        /// <param name="eventStream"></param>
        /// <param name="concurrentException"></param>
        public void RetryCommand(CommandInfo commandInfo, EventStream eventStream, ConcurrentException concurrentException)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = ENodeConfiguration.Instance.GetRetryCommandQueue();
            }
            var command = commandInfo.Command;

            if (commandInfo.RetriedCount < command.RetryCount)
            {
                _retryCommandQueue.Enqueue(new Message<ICommand>(Guid.NewGuid(), commandInfo.Command, _retryCommandQueue.Name));
                commandInfo.IncreaseRetriedCount();
            }
            else
            {
                _logger.InfoFormat("{0} retried count reached to its max retry count {1}.", command.GetType().Name, command.RetryCount);
            }
        }
    }
}
