namespace ENode.Messaging
{
    /// <summary>Represents a local in-memory based message handler.
    /// </summary>
    public interface IMessageHandler<TMessagePayload>
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        void Handle(Message<TMessagePayload> message);
    }
}
