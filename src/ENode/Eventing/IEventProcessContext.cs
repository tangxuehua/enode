namespace ENode.Eventing
{
    /// <summary>Represents a context environment for event processor processing committed event stream.
    /// </summary>
    public interface IEventProcessContext
    {
        /// <summary>Notify the given event stream has been processed.
        /// </summary>
        /// <param name="eventStream">The processed event stream.</param>
        void OnEventProcessed(EventStream eventStream);
    }
}
