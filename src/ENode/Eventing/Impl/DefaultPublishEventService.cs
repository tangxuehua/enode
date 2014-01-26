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

        public void PublishEvent(EventStream eventStream, ProcessingCommand processingCommand)
        {
            var publishEvents = new Func<bool>(() =>
            {
                try
                {
                    _eventPublisher.PublishEvent(eventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("PublishEvents", publishEvents, 3, new ActionInfo("PublishEventsCallback", data =>
            {
                _processingCommandCache.Remove(eventStream.CommandId);
                processingCommand.CommandExecuteContext.OnCommandExecuted(processingCommand.Command);
                return true;
            }, null, null));
        }
    }
}
