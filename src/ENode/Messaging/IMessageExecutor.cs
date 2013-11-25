namespace ENode.Messaging
{
    /// <summary>Represents a local in-memory based message executor.
    /// </summary>
    public interface IMessageExecutor<TMessagePayload> where TMessagePayload : class, IMessagePayload
    {
        /// <summary>Execute the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void Execute(Message<TMessagePayload> message);
    }
}
