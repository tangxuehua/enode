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
        public void RetryCommand(CommandInfo commandInfo, EventStream eventStream, ErrorInfo errorInfo, ActionInfo retrySuccessCallbackAction)
        {
            if (_retryCommandQueue == null)
            {
                _retryCommandQueue = Configuration.Instance.GetRetryCommandQueue();
            }
            var command = commandInfo.Command;

            Action<CommandInfo, ActionInfo> actionAfterCommandRetried = (currentCommandInfo, callbackActionInfo) =>
            {
                currentCommandInfo.IncreaseRetriedCount();
                _logger.InfoFormat("Sent {0} to command retry queue for {1} time.", currentCommandInfo.Command.GetType().Name, currentCommandInfo.RetriedCount);
                callbackActionInfo.Action(callbackActionInfo.Data);
            };

            if (commandInfo.RetriedCount < command.RetryCount)
            {
                if (_retryService.TryAction("TryEnqueueCommand", () => TryEnqueueCommand(command), 2))
                {
                    actionAfterCommandRetried(commandInfo, retrySuccessCallbackAction);
                }
                else
                {
                    _retryService.RetryInQueue(
                        new ActionInfo(
                            "TryEnqueueCommand",
                            (obj) => TryEnqueueCommand(obj as ICommand),
                            command,
                            new ActionInfo(
                                "TryEnqueueCommandFinishedAction",
                                (obj) =>
                                {
                                    var data = obj as dynamic;
                                    var currentCommandInfo = data.CommandInfo as CommandInfo;
                                    var callbackActionInfo = data.Callback as ActionInfo;
                                    actionAfterCommandRetried(currentCommandInfo, callbackActionInfo);
                                    return true;
                                },
                                new { CommandInfo = commandInfo, Callback = retrySuccessCallbackAction },
                                null)));
                }
            }
            else
            {
                _commandAsyncResultManager.TryComplete(commandInfo.Command.Id, eventStream.AggregateRootId, errorInfo.ErrorMessage, errorInfo.Exception);
            }
        }

        private bool TryEnqueueCommand(ICommand command)
        {
            try
            {
                _retryCommandQueue.Enqueue(command);
                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Exception raised when tring to enqueue the command to the retry command queue. commandType{0}, commandId:{1}", command.GetType().Name, command.Id);
                _logger.Error(errorMessage, ex);
                return false;
            }
        }
    }
}
