using System;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;

namespace ENode.Eventing.Impl
{
    public class DefaultPublishEventService : IPublishEventService
    {
        private readonly IActionExecutionService _actionExecutionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger _logger;

        public DefaultPublishEventService(IActionExecutionService actionExecutingService, IEventPublisher eventPublisher, ILoggerFactory loggerFactory)
        {
            _actionExecutionService = actionExecutingService;
            _eventPublisher = eventPublisher;
            _logger = loggerFactory.Create(GetType().Name);
        }

        public void PublishEvent(EventProcessingContext context)
        {
            var publishEvents = new Func<bool>(() =>
            {
                try
                {
                    _eventPublisher.PublishEvent(context.EventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", context.EventStream), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("PublishEvents", publishEvents, 3, new ActionInfo("PublishEventsCallback", data =>
            {
                var currentContext = data as EventProcessingContext;
                var hasProcessCompletedEvent = currentContext.EventStream.Events.Any(x => x is IProcessCompletedEvent);
                currentContext.ProcessingCommand.CommandExecuteContext.OnCommandExecuted(new CommandResult(currentContext.ProcessingCommand.Command, hasProcessCompletedEvent));
                return true;
            }, context, null));
        }
    }
}
