using System;
using System.Collections.Generic;
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

        public void PublishEvent(IDictionary<string, string> contextItems, EventStream eventStream)
        {
            var publishEvents = new Func<bool>(() =>
            {
                try
                {
                    _eventPublisher.PublishEvent(contextItems, eventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("PublishEvents", publishEvents, 3, null);
        }
    }
}
