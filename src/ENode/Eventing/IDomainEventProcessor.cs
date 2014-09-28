namespace ENode.Eventing
{
    /// <summary>Represents a processor to process domain event stream.
    /// </summary>
    public interface IDomainEventProcessor
    {
        /// <summary>Gets or sets the name of the event processor.
        /// </summary>
        string Name { get; set; }
        /// <summary>Process the domain event stream.
        /// </summary>
        void Process(DomainEventStream eventStream, IDomainEventProcessContext context);
    }
}
