namespace ENode.Commanding
{
    /// <summary>Represents a command which associated with an aggregate.
    /// </summary>
    public interface IAggregateCommand : ICommand
    {
        /// <summary>Represents the aggregate root id.
        /// </summary>
        string AggregateRootId { get; }
    }
}
