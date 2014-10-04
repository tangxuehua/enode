namespace ENode.Eventing
{
    /// <summary>Represents an event.
    /// </summary>
    public interface IEvent
    {
        /// <summary>Represents the unique identifier of the event.
        /// </summary>
        string Id { get; }
    }
}
