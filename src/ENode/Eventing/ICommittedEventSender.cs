using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a sender to send the committed event stream to a specific message queue.
    /// </summary>
    public interface ICommittedEventSender : IMessageSender<EventStream>
    {
    }
}
