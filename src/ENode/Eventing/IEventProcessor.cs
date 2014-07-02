namespace ENode.Eventing
{
    /// <summary>Represents a processor to process committed event stream.
    /// </summary>
    public interface IEventProcessor
    {
        /// <summary>Gets or sets the name of the event processor.
        /// </summary>
        string Name { get; set; }
        /// <summary>Process the committed event stream.
        /// </summary>
        void Process(EventStream eventStream, IEventProcessContext context);
    }
}
