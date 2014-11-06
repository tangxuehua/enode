namespace ENode.Eventing
{
    /// <summary>Represents a publisher to publish event.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>Publish the given event to all the event handlers.
        /// </summary>
        /// <param name="evnt"></param>
        void Publish(IEvent evnt);
    }
}
