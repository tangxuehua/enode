using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a event stream queue.
    /// </summary>
    public interface IEventQueue : IQueue<EventStream>
    {
    }
}
