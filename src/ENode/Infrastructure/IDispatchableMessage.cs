namespace ENode.Infrastructure
{
    /// <summary>Represents a dispatchable message.
    /// </summary>
    public interface IDispatchableMessage
    {
        /// <summary>Represents the unique identifier of the message.
        /// </summary>
        string Id { get; }
    }
}
