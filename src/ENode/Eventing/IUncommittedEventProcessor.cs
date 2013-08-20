using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a processor to process uncommitted event stream.
    /// </summary>
    public interface IUncommittedEventProcessor : IMessageProcessor<IUncommittedEventQueue, EventStream>
    {
    }
}
