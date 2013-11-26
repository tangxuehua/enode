namespace ENode.Messaging
{
    /// <summary>Represents a sender to send a payload a to a specific message queue.
    /// </summary>
    public interface IMessageSender<TMessagePayload> where TMessagePayload : class, IPayload
    {
        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        void Send(TMessagePayload payload);
    }
}
