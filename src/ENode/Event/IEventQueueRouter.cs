namespace ENode.Eventing
{
    /// <summary>Represents a router to route a available event queue for event stream message.
    /// </summary>
    public interface IEventQueueRouter
    {
        /// <summary>Route a available event queue for the given event stream message.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IEventQueue Route(EventStream stream);
    }
}
