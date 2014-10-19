namespace ENode.Infrastructure
{
    /// <summary>Represents a message processor.
    /// </summary>
    public interface IMessageProcessor<TMessage> where TMessage : class
    {
        /// <summary>Gets or sets the name of the processor.
        /// </summary>
        string Name { get; set; }
        /// <summary>Process the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        void Process(TMessage message, IMessageProcessContext<TMessage> context);
    }
}
