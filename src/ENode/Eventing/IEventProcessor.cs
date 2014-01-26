using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a processor to process committed event stream.
    /// </summary>
    public interface IEventProcessor
    {
        /// <summary>Process the committed event stream.
        /// </summary>
        void Process(EventStream eventStream, IEventProcessContext context);
    }
}
