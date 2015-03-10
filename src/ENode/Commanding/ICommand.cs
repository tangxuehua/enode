using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand : IMessage
    {
        /// <summary>Represents the associated aggregate root string id.
        /// </summary>
        string AggregateRootId { get; }
    }
}
