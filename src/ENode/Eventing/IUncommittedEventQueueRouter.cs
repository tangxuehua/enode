using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a router to route an available uncommitted event queue for event stream.
    /// </summary>
    public interface IUncommittedEventQueueRouter : IMessageQueueRouter<IUncommittedEventQueue, EventStream>
    {
    }
}
