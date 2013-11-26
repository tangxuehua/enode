using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a sender to send the uncommitted event stream to a specific message queue.
    /// </summary>
    public interface IUncommittedEventSender : IMessageSender<EventStream>
    {
    }
}
