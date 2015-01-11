using ECommon.Logging;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultDomainEventStreamProcessor : IProcessor<DomainEventStream>
    {
        #region Private Variables

        private readonly IEventDispatcher _eventDispatcher;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly ILogger _logger;

        #endregion

        public string Name { get; set; }

        #region Constructors

        public DefaultDomainEventStreamProcessor(IEventPublishInfoStore eventPublishInfoStore, IEventDispatcher eventDispatcher, ILoggerFactory loggerFactory)
        {
            Name = GetType().Name;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventDispatcher = eventDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        public void Start()
        {
            _eventDispatcher.Start();
        }
        public void Process(DomainEventStream domainEventStream, IProcessContext<DomainEventStream> context)
        {
            _eventDispatcher.EnqueueProcessingContext(new DomainEventStreamProcessingContext(this, domainEventStream, context));
        }

        #region Private Methods

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

        #endregion

        class DomainEventStreamProcessingContext : ProcessingContext<DomainEventStream>
        {
            private DefaultDomainEventStreamProcessor _processor;

            public DomainEventStreamProcessingContext(DefaultDomainEventStreamProcessor processor, DomainEventStream domainEventStream, IProcessContext<DomainEventStream> eventProcessContext)
                : base("ProcessDomainEventStream", domainEventStream, eventProcessContext)
            {
                _processor = processor;
            }
            public override object GetHashKey()
            {
                return Message.AggregateRootId;
            }
            public override bool Process()
            {
                var domainEventStream = Message;
                var lastPublishedVersion = _processor._eventPublishInfoStore.GetEventPublishedVersion(_processor.Name, domainEventStream.AggregateRootId);

                if (lastPublishedVersion + 1 == domainEventStream.Version)
                {
                    return _processor._eventDispatcher.DispatchEventsToHandlers(domainEventStream);
                }
                else if (lastPublishedVersion + 1 < domainEventStream.Version)
                {
                    _processor._logger.DebugFormat("Wait to publish, [aggregateRootId={0},lastPublishedVersion={1},currentVersion={2}]", domainEventStream.AggregateRootId, lastPublishedVersion, domainEventStream.Version);
                    return false;
                }

                return true;
            }
            protected override void OnMessageProcessed()
            {
                _processor.UpdatePublishedVersion(Message);
                base.OnMessageProcessed();
            }
        }
    }
}
