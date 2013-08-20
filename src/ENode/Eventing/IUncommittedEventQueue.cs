using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a queue of uncommitted event stream.
    /// </summary>
    public interface IUncommittedEventQueue : IMessageQueue<EventStream>
    {
    }
}
