using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventProcessor : IProcessor<IEvent>
    {
        #region Private Variables

        private readonly IEventDispatcher _eventDispatcher;

        #endregion

        public string Name { get; set; }

        #region Constructors

        public DefaultEventProcessor(IEventDispatcher eventDispatcher)
        {
            Name = GetType().Name;
            _eventDispatcher = eventDispatcher;
        }

        #endregion

        public void Start()
        {
            _eventDispatcher.Start();
        }
        public void Process(IEvent evnt, IProcessContext<IEvent> context)
        {
            _eventDispatcher.EnqueueProcessingContext(new EventProcessingContext(this, evnt, context));
        }

        class EventProcessingContext : ProcessingContext<IEvent>
        {
            private DefaultEventProcessor _processor;

            public EventProcessingContext(DefaultEventProcessor processor, IEvent evnt, IProcessContext<IEvent> eventProcessContext)
                : base("ProcessEvent", evnt, eventProcessContext)
            {
                _processor = processor;
            }
            public override object GetHashKey()
            {
                return Message is IDomainEvent ? ((IDomainEvent)Message).AggregateRootId : Message.Id;
            }
            public override bool Process()
            {
                return _processor._eventDispatcher.DispatchEventToHandlers(Message);
            }
        }
    }
}
