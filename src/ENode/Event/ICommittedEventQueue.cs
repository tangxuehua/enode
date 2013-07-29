using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a queue of committed event stream.
    /// </summary>
    public interface ICommittedEventQueue : IMessageQueue<EventStream>
    {
    }
}
