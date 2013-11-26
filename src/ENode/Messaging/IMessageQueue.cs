namespace ENode.Messaging
{
    /// <summary>Represents a local in-memory based message queue.
    /// </summary>
    public interface IMessageQueue<TMessagePayload> where TMessagePayload : class, IPayload
    {
        /// <summary>The name of the queue.
        /// </summary>
        string Name { get; }
        /// <summary>Enqueue a message to the queue.
        /// </summary>
        /// <param name="message"></param>
        void Enqueue(Message<TMessagePayload> message);
        /// <summary>Dequeue the top message from the queue. If no message exist, block the current thread.
        /// </summary>
        Message<TMessagePayload> Dequeue();
    }
}
