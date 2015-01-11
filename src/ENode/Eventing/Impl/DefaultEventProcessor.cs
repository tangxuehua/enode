using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventProcessor : AbstractParallelProcessor<IEvent>
    {
        #region Private Variables

        private readonly IDispatcher<IEvent> _eventDispatcher;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultEventProcessor(IDispatcher<IEvent> eventDispatcher, ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.EventProcessorParallelThreadCount, "ProcessEvent")
        {
            _eventDispatcher = eventDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<IEvent> CreateQueueMessage(IEvent message, IProcessContext<IEvent> processContext)
        {
            var hashKey = message is IDomainEvent ? ((IDomainEvent)message).AggregateRootId : message.Id;
            return new QueueMessage<IEvent>(hashKey, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<IEvent> queueMessage)
        {
            var evnt = queueMessage.Payload;
            var success = _eventDispatcher.DispatchMessage(evnt);

            if (!success)
            {
                _logger.ErrorFormat("Process event failed, eventId:{0}, eventType:{1}, retryTimes:{2}", evnt.Id, evnt.GetType().Name, queueMessage.RetryTimes);
            }

            OnMessageHandled(success, queueMessage);
        }
    }
}
