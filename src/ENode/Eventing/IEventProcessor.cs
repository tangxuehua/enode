namespace ENode.Eventing
{
    /// <summary>Represents a processor to process event stream.
    /// </summary>
    public interface IEventProcessor
    {
        /// <summary>Process the event stream.
        /// </summary>
        void Process(EventStream eventStream, IEventProcessContext context);
    }
}
