using System;
using ECommon.Logging;
using ENode.Configurations;
using ENode.Eventing;
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
        /// <param name="eventCommittingContext"></param>
        public void RetryCommand(CommandInfo commandInfo, EventCommittingContext eventCommittingContext)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = ENodeConfiguration.Instance.GetRetryCommandQueue();
            }
            var command = commandInfo.Command;

            if (commandInfo.RetriedCount < command.RetryCount)
            {
                _retryCommandQueue.Enqueue(new Message<EventCommittingContext>(Guid.NewGuid(), eventCommittingContext, _retryCommandQueue.Name));
                commandInfo.IncreaseRetriedCount();
            }
            else
            {
                _logger.InfoFormat("{0} retried count reached to its max retry count {1}.", command.GetType().Name, command.RetryCount);
            }
        }
    }
}
