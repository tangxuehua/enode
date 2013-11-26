namespace ENode.Messaging
{
    /// <summary>Represents a local in-memory based message handler.
    /// </summary>
    public interface IMessageHandler<TMessagePayload> where TMessagePayload : class, IPayload
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void Handle(Message<TMessagePayload> message);
    }
}
