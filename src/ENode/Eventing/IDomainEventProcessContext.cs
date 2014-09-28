namespace ENode.Eventing
{
    /// <summary>Represents a context environment for processing domain event stream.
    /// </summary>
    public interface IDomainEventProcessContext
    {
        /// <summary>Notify the given event stream has been processed.
        /// </summary>
        /// <param name="eventStream">The processed event stream.</param>
        void OnEventProcessed(DomainEventStream eventStream);
    }
}
