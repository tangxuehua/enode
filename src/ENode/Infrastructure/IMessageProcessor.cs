namespace ENode.Infrastructure
{
    /// <summary>Represents a message processor.
    /// </summary>
    public interface IMessageProcessor<TMessage> where TMessage : class
    {
        /// <summary>Process the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        void Process(TMessage message, IMessageProcessContext<TMessage> context);
    }
}
