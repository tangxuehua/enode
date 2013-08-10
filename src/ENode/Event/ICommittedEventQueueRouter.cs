namespace ENode.Eventing {
    /// <summary>Represents a router to route a available committed event queue for event stream message.
    /// </summary>
    public interface ICommittedEventQueueRouter {
        /// <summary>Route a available committed event queue for the given event stream message.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        ICommittedEventQueue Route(EventStream stream);
    }
}
