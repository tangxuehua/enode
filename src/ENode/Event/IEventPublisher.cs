namespace ENode.Eventing
{
    /// <summary>An event publisher interface to publish the committed event to event handlers.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>Publish a given committed event stream to all event handlers.
        /// </summary>
        void Publish(EventStream stream);
    }
}
