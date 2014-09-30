namespace ENode.Infrastructure
{
    /// <summary>Represents a message publisher.
    /// </summary>
    public interface IMessagePublisher<TMessage> where TMessage : class
    {
        /// <summary>Publish the given message to all the message handlers.
        /// </summary>
        /// <param name="message"></param>
        void Publish(TMessage message);
    }
}
