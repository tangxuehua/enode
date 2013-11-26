namespace ENode.Messaging
{
    /// <summary>Represents a local memory-based message queue processor.
    /// </summary>
    public interface IMessageProcessor<out TQueue, TMessagePayload>
        where TQueue : class, IMessageQueue<TMessagePayload>
        where TMessagePayload : class, IPayload
    {
        /// <summary>Represents the binding message queue.
        /// </summary>
        TQueue BindingQueue { get; }
        /// <summary>Start the message processor, and it will start to fetch message from the binding message queue.
        /// </summary>
        void Start();
    }
}
