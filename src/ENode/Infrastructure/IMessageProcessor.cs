namespace ENode.Infrastructure
{
    /// <summary>Represents a message processor.
    /// </summary>
    public interface IMessageProcessor<X, Y> where X : class, IProcessingMessage<X, Y> where Y : IMessage
    {
        /// <summary>Process the given message.
        /// </summary>
        /// <param name="processingMessage"></param>
        void Process(X processingMessage);
        /// <summary>Start the processor.
        /// </summary>
        void Start();
        /// <summary>Stop the processor.
        /// </summary>
        void Stop();
    }
}
