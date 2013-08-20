namespace ENode.Eventing
{
    /// <summary>Represents an event publisher to publish the committed event stream to event handlers.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>Publish a given committed event stream to all the event handlers.
        /// </summary>
        void Publish(EventStream stream);
    }
}
