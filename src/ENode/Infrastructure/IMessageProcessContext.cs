namespace ENode.Infrastructure
{
    /// <summary>Represents a context environment for processing message.
    /// </summary>
    public interface IMessageProcessContext<TMessage> where TMessage : class
    {
        /// <summary>Notify the given message has been processed.
        /// </summary>
        /// <param name="message">The processed message.</param>
        void OnMessageProcessed(TMessage message);
    }
}
