namespace ENode.Infrastructure
{
    /// <summary>Represents a message publisher.
    /// </summary>
    public interface IPublisher<TMessage> where TMessage : class
    {
        /// <summary>Publish the given message to all the subscribers.
        /// </summary>
        /// <param name="message"></param>
        void Publish(TMessage message);
    }
}
