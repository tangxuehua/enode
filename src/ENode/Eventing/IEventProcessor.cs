using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a processor to process uncommitted event stream.
    /// </summary>
    public interface IEventProcessor
    {
        /// <summary>Initialize the processor.
        /// </summary>
        void Initialize();
        /// <summary>Start the processor.
        /// </summary>
        void Start();
    }
}
