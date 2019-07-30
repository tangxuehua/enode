namespace ENode.Eventing
{
    /// <summary>Represents a processor to process domain event stream message.
    /// </summary>
    public interface IProcessingDomainEventStreamMessageProcessor
    {
        /// <summary>Process the given message.
        /// </summary>
        /// <param name="processingDomainEventStreamMessage"></param>
        void Process(ProcessingDomainEventStreamMessage processingDomainEventStreamMessage);
        /// <summary>Start the processor.
        /// </summary>
        void Start();
        /// <summary>Stop the processor.
        /// </summary>
        void Stop();
    }
}
