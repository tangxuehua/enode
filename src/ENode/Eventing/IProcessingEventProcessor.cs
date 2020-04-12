namespace ENode.Eventing
{
    /// <summary>Represents a processor to process event.
    /// </summary>
    public interface IProcessingEventProcessor
    {
        /// <summary>The name of the processor
        /// </summary>
        string Name { get; }
        /// <summary>Process the given processingEvent.
        /// </summary>
        /// <param name="processingEvent"></param>
        void Process(ProcessingEvent processingEvent);
        /// <summary>Start the processor.
        /// </summary>
        void Start();
        /// <summary>Stop the processor.
        /// </summary>
        void Stop();
    }
}
