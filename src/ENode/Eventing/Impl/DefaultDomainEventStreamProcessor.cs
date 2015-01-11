using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultDomainEventStreamProcessor : AbstractParallelProcessor<DomainEventStream>
    {
        #region Private Variables

        private readonly IDispatcher<IEvent> _eventDispatcher;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultDomainEventStreamProcessor(IEventPublishInfoStore eventPublishInfoStore, IDispatcher<IEvent> eventDispatcher, ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.DomainEventStreamProcessorParallelThreadCount, "ProcessDomainEventStream")
        {
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventDispatcher = eventDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<DomainEventStream> CreateQueueMessage(DomainEventStream message, IProcessContext<DomainEventStream> processContext)
        {
            return new QueueMessage<DomainEventStream>(message.AggregateRootId, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<DomainEventStream> queueMessage)
        {
            var domainEventStream = queueMessage.Payload;
            var lastPublishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(Name, domainEventStream.AggregateRootId);
            var success = true;

            if (lastPublishedVersion + 1 == domainEventStream.Version)
            {
                success = _eventDispatcher.DispatchMessages(domainEventStream.Events);
                if (success)
                {
                    UpdatePublishedVersion(domainEventStream);
                }
                else
                {
                    _logger.ErrorFormat("Process domain event stream failed, domainEventStream:{0}, retryTimes:{1}", domainEventStream, queueMessage.RetryTimes);
                }
            }
            else if (lastPublishedVersion + 1 < domainEventStream.Version)
            {
                _logger.DebugFormat("Wait to publish, [aggregateRootId={0},lastPublishedVersion={1},currentVersion={2}]", domainEventStream.AggregateRootId, lastPublishedVersion, domainEventStream.Version);
                success = false;
            }

            OnMessageHandled(success, queueMessage);
        }

        private void UpdatePublishedVersion(DomainEventStream stream)
        {
            if (stream.Version == 1)
            {
                _eventPublishInfoStore.InsertPublishedVersion(Name, stream.AggregateRootId);
            }
            else
            {
                _eventPublishInfoStore.UpdatePublishedVersion(Name, stream.AggregateRootId, stream.Version);
            }
        }
    }
}
