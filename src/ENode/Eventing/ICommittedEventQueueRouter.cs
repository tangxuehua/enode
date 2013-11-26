using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a router to route an available committed event queue for the given event stream.
    /// </summary>
    public interface ICommittedEventQueueRouter : IMessageQueueRouter<ICommittedEventQueue, EventStream>
    {
    }
}
