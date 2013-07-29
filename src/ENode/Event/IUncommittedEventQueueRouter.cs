namespace ENode.Eventing
{
    /// <summary>Represents a router to route a available uncommitted event queue for event stream message.
    /// </summary>
    public interface IUncommittedEventQueueRouter
    {
        /// <summary>Route a available uncommitted event queue for the given event stream message.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IUncommittedEventQueue Route(EventStream stream);
    }
}
