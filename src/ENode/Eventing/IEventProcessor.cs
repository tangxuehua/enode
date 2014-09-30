using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a processor to process event stream.
    /// </summary>
    public interface IEventProcessor : IMessageProcessor<IEventStream>
    {
        /// <summary>Gets or sets the name of the event processor.
        /// </summary>
        string Name { get; set; }
    }
}
