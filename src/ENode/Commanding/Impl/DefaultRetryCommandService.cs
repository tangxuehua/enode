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
        private readonly ICommandAsyncResultManager _commandAsyncResultManager;
        private readonly IRetryService _retryService;
        private readonly ILogger _logger;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="commandAsyncResultManager"></param>
        /// <param name="retryService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultRetryCommandService(ICommandAsyncResultManager commandAsyncResultManager, IRetryService retryService, ILoggerFactory loggerFactory)
        {
            _commandAsyncResultManager = commandAsyncResultManager;
            _retryService = retryService;
            _logger = loggerFactory.Create(GetType().Name);
        }

        /// <summary>Retry the given command.
        /// </summary>
        /// <param name="commandInfo"></param>
        /// <param name="eventStream"></param>
        /// <param name="errorInfo"></param>
        /// <param name="retrySuccessCallbackAction"></param>
        public void RetryCommand(CommandInfo commandInfo, EventStream eventStream, ErrorInfo errorInfo, Action retrySuccessCallbackAction)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = Configuration.Instance.GetRetryCommandQueue();
            }
            var command = commandInfo.Command;

            if (commandInfo.RetriedCount < command.RetryCount)
            {
                _retryService.TryAction("TryEnqueueCommand", () => TryEnqueueCommand(commandInfo), 3, retrySuccessCallbackAction);
            }
            else
            {
                _commandAsyncResultManager.TryComplete(command.Id, eventStream.AggregateRootId, errorInfo);
                _logger.InfoFormat("{0} retried count reached to its max retry count {1}.", command.GetType().Name, command.RetryCount);
                if (retrySuccessCallbackAction != null)
                {
                    retrySuccessCallbackAction();
                }
            }
        }

        private bool TryEnqueueCommand(CommandInfo commandInfo)
        {
            try
            {
                _retryCommandQueue.Enqueue(commandInfo.Command);
                commandInfo.IncreaseRetriedCount();
                _logger.InfoFormat("Sent {0} to command retry queue for {1} time.", commandInfo.Command.GetType().Name, commandInfo.RetriedCount);
                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Exception raised when tring to enqueue the command to the retry command queue. commandType{0}, commandId:{1}", commandInfo.Command.GetType().Name, commandInfo.Command.Id);
                _logger.Error(errorMessage, ex);
                return false;
            }
        }
    }
}
