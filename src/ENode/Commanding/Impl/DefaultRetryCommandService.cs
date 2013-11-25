using System;
using ENode.Eventing;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Logging;
using ENode.Messaging;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IRetryCommandService.
    /// </summary>
    public class DefaultRetryCommandService : IRetryCommandService
    {
        private ICommandTaskManager _commandTaskManager;
        private ICommandQueue _retryCommandQueue;
        private readonly ILogger _logger;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultRetryCommandService(ICommandTaskManager commandTaskManager, ILoggerFactory loggerFactory)
        {
            _commandTaskManager = commandTaskManager;
            _logger = loggerFactory.Create(GetType().Name);
        }

        /// <summary>Retry the given command.
        /// </summary>
        /// <param name="commandInfo"></param>
        /// <param name="eventStream"></param>
        /// <param name="errorInfo"></param>
        public void RetryCommand(CommandInfo commandInfo, EventStream eventStream, ConcurrentException concurrentException)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = Configuration.Instance.GetRetryCommandQueue();
            }
            var command = commandInfo.Command;

            if (commandInfo.RetriedCount < command.RetryCount)
            {
                _retryCommandQueue.Enqueue(new Message<ICommand>(Guid.NewGuid(), commandInfo.Command, _retryCommandQueue.Name));
                commandInfo.IncreaseRetriedCount();
            }
            else
            {
                _logger.ErrorFormat("{0} retried count reached to its max retry count {1}.", command.GetType().Name, command.RetryCount);
                _commandTaskManager.CompleteCommandTask(command.Id, concurrentException);
            }
        }
    }
}
