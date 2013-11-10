namespace ENode.Messaging
{
    /// <summary>Represents a message queue for storing messages and also support transaction and durable messages.
    /// </summary>
    public interface IMessageQueue<T> where T : class, IMessage
    {
        /// <summary>The name of the queue.
        /// </summary>
        string Name { get; }
        /// <summary>Initialize the queue.
        /// </summary>
        void Initialize();
        /// <summary>Add an message to the queue.
        /// <remarks>
        /// The message will be persisted first, and then be added to in-memory queue.
        /// </remarks>
        /// </summary>
        /// <param name="message"></param>
        void Enqueue(T message);
        /// <summary>Remove the top message from the queue.
        /// </summary>
        T Dequeue();
        /// <summary>Notify the queue to delete the given message.
        /// </summary>
        /// <param name="message"></param>
        void Delete(T message);
    }
}
