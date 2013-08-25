namespace ENode.Eventing
{
    /// <summary>Represents a router to route an available committed event queue for the given event stream.
    /// </summary>
    public interface ICommittedEventQueueRouter
    {
        /// <summary>Route an available committed event queue for the given event stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        ICommittedEventQueue Route(EventStream stream);
    }
}
