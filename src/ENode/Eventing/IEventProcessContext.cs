namespace ENode.Eventing
{
    /// <summary>Represents the event processing context.
    /// </summary>
    public interface IEventProcessContext
    {
        /// <summary>Notify the event has been processed.
        /// </summary>
        void NotifyEventProcessed();
    }
}
