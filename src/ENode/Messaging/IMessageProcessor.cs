namespace ENode.Messaging
{
    /// <summary>Represents a message processor.
    /// </summary>
    public interface IMessageProcessor<out TQueue, TMessage>
        where TQueue : class, IMessageQueue<TMessage>
        where TMessage : class, IMessage
    {
        /// <summary>Represents the binding message queue.
        /// </summary>
        TQueue BindingQueue { get; }
        /// <summary>Initialize the message processor.
        /// </summary>
        void Initialize();
        /// <summary>Start the message processor, and it will start to fetch message from the binding message queue.
        /// </summary>
        void Start();
    }
}
