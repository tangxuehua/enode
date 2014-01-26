using System;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;

namespace ENode.Eventing.Impl
{
    public class DefaultPublishEventService : IPublishEventService
    {
        private readonly IActionExecutionService _actionExecutionService;
        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger _logger;

        public DefaultPublishEventService(IActionExecutionService actionExecutingService, IProcessingCommandCache processingCommandCache, IEventPublisher eventPublisher, ILoggerFactory loggerFactory)
        {
            _actionExecutionService = actionExecutingService;
            _processingCommandCache = processingCommandCache;
            _eventPublisher = eventPublisher;
            _logger = loggerFactory.Create(GetType().Name);
        }

        public void PublishEvent(EventStream eventStream, ICommand command, ICommandExecuteContext commandExecuteContext)
        {
            var publishEvents = new Func<bool>(() =>
            {
                try
                {
                    _eventPublisher.Send(eventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream.GetStreamInformation()), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("PublishEvents", publishEvents, 3, new ActionInfo("PublishEventsCallback", data =>
            {
                _processingCommandCache.TryRemove(eventStream.CommandId);
                commandExecuteContext.OnCommandExecuted(command);
                return true;
            }, null, null));
        }
    }
}
