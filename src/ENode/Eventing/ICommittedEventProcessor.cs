using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a processor to process committed event streams.
    /// </summary>
    public interface ICommittedEventProcessor : IMessageProcessor<ICommittedEventQueue, EventStream>
    {
    }
}
