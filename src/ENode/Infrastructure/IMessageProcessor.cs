namespace ENode.Infrastructure
{
    /// <summary>Represents a message processor.
    /// </summary>
    public interface IMessageProcessor<X, Y, Z> where X : class, IProcessingMessage<X, Y, Z> where Y : IMessage
    {
        /// <summary>Process the given message.
        /// </summary>
        /// <param name="processingMessage"></param>
        void Process(X processingMessage);
    }
}
