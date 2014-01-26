namespace ENode.Eventing
{
    /// <summary>An internal class to contains the context information when processing the committed event stream.
    /// </summary>
    internal class EventProcessingContext
    {
        public EventStream EventStream { get; private set; }
        public IEventProcessContext EventProcessContext { get; private set; }

        public EventProcessingContext(EventStream eventStream, IEventProcessContext eventProcessContext)
        {
            EventStream = eventStream;
            EventProcessContext = eventProcessContext;
        }
    }
}
