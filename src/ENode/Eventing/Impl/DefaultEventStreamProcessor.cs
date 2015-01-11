using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventStreamProcessor : IProcessor<EventStream>
    {
        #region Private Variables

        private readonly IEventDispatcher _eventDispatcher;

        #endregion

        public string Name { get; set; }

        #region Constructors

        public DefaultEventStreamProcessor(IEventDispatcher eventDispatcher)
        {
            Name = GetType().Name;
            _eventDispatcher = eventDispatcher;
        }

        #endregion

        public void Start()
        {
            _eventDispatcher.Start();
        }
        public void Process(EventStream eventStream, IProcessContext<EventStream> context)
        {
            _eventDispatcher.EnqueueProcessingContext(new EventStreamProcessingContext(this, eventStream, context));
        }

        class EventStreamProcessingContext : ProcessingContext<EventStream>
        {
            private DefaultEventStreamProcessor _processor;

            public EventStreamProcessingContext(DefaultEventStreamProcessor processor, EventStream eventStream, IProcessContext<EventStream> eventProcessContext)
                : base("ProcessEventStream", eventStream, eventProcessContext)
            {
                _processor = processor;
            }
            public override object GetHashKey()
            {
                return Message.CommandId;
            }
            public override bool Process()
            {
                return _processor._eventDispatcher.DispatchEventsToHandlers(Message);
            }
        }
    }
}
