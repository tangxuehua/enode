namespace ENode.Infrastructure
{
    public interface IDispatchableMessage
    {
        /// <summary>Represents the unique identifier of the message.
        /// </summary>
        string Id { get; }
    }
}
