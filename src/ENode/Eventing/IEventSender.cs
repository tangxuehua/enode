namespace ENode.Eventing
{
    /// <summary>Represents an event sender to send the uncommitted event stream to process asynchronously.
    /// </summary>
    public interface IEventSender
    {
        /// <summary>Send the uncommitted event stream to process asynchronously.
        /// </summary>
        void Send(EventStream eventStream);
    }
}
