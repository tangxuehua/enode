using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a processor to process committed event stream.
    /// </summary>
    public interface IEventProcessor : IMessageProcessor<IEventQueue, EventStream>
    {
    }
}
