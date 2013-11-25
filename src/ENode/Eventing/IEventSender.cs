using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a sender to send the uncommitted event stream to process asynchronously.
    /// </summary>
    public interface IEventSender
    {
        /// <summary>Send the uncommitted event stream to process asynchronously.
        /// </summary>
        void Send(EventStream eventStream);
    }
}
