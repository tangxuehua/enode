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
        private readonly IAggregatePublishVersionStore _aggregatePublishVersionStore;
        private readonly ILogger _logger;
        private readonly IOHelper _ioHelper;

        #endregion

        #region Constructors

        public DefaultDomainEventStreamProcessor(IAggregatePublishVersionStore aggregatePublishVersionStore, IDispatcher<IEvent> eventDispatcher, IOHelper ioHelper, ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.DomainEventStreamProcessorParallelThreadCount, "ProcessDomainEventStream")
        {
            _aggregatePublishVersionStore = aggregatePublishVersionStore;
            _eventDispatcher = eventDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
            _ioHelper = ioHelper;
        }

        #endregion

        protected override QueueMessage<DomainEventStream> CreateQueueMessage(DomainEventStream message, IProcessContext<DomainEventStream> processContext)
        {
            return new QueueMessage<DomainEventStream>(message.AggregateRootId, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<DomainEventStream> queueMessage)
        {
            var domainEventStream = queueMessage.Payload;
            var lastPublishedVersion = GetPublishVersion(domainEventStream.AggregateRootId);
            if (lastPublishedVersion == -1)
            {
                OnMessageHandled(true, queueMessage);
                return;
            }

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

            OnMessageHandled(!success, queueMessage);
        }

        private int GetPublishVersion(string aggregateRootId)
        {
            var result = _ioHelper.TryIOFuncRecursively<int>("GetAggregatePublishVersion", () => aggregateRootId, () =>
            {
                return _aggregatePublishVersionStore.GetVersion(Name, aggregateRootId);
            });
            if (!result.Success)
            {
                return -1;
            }
            return result.Data;
        }
        private void UpdatePublishedVersion(DomainEventStream stream)
        {
            if (stream.Version == 1)
            {
                _ioHelper.TryIOActionRecursively("InsertFirstAggregatePublishVersion", () => stream.AggregateRootId, () =>
                {
                    _aggregatePublishVersionStore.InsertFirstVersion(Name, stream.AggregateRootId);
                });
            }
            else
            {
                _ioHelper.TryIOActionRecursively("UpdateAggregatePublishVersion", () => stream.AggregateRootId, () =>
                {
                    _aggregatePublishVersionStore.UpdateVersion(Name, stream.AggregateRootId, stream.Version);
                });
            }
        }
    }
}
