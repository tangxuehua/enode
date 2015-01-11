namespace ENode.Infrastructure
{
    /// <summary>Represents a message processor.
    /// </summary>
    public interface IProcessor<T> where T : class
    {
        /// <summary>Gets or sets the name of the processor.
        /// </summary>
        string Name { get; set; }
        /// <summary>Start the processor.
        /// </summary>
        void Start();
        /// <summary>Process the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="processContext"></param>
        void Process(T message, IProcessContext<T> processContext);
    }
}
