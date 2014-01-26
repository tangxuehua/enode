using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a publisher to publish the committed event stream to a specific message queue.
    /// </summary>
    public interface IEventPublisher : IMessageSender<EventStream>
    {
    }
}
