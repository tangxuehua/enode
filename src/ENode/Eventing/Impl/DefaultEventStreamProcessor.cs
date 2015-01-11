using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventStreamProcessor : AbstractParallelProcessor<EventStream>
    {
        #region Private Variables

        private readonly IDispatcher<IEvent> _eventDispatcher;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultEventStreamProcessor(IDispatcher<IEvent> eventDispatcher, ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.EventStreamProcessorParallelThreadCount, "ProcessEventStream")
        {
            _eventDispatcher = eventDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<EventStream> CreateQueueMessage(EventStream message, IProcessContext<EventStream> processContext)
        {
            return new QueueMessage<EventStream>(message.CommandId, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<EventStream> queueMessage)
        {
            var eventStream = queueMessage.Payload;
            var success = _eventDispatcher.DispatchMessages(eventStream.Events);

            if (!success)
            {
                _logger.ErrorFormat("Process event stream failed, eventStream:{0}, retryTimes:{1}", eventStream, queueMessage.RetryTimes);
            }

            OnMessageHandled(success, queueMessage);
        }
    }
}
