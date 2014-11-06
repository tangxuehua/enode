
namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent : IEvent
    {
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        string AggregateRootId { get; }
        /// <summary>Represents the version of the domain event.
        /// </summary>
        int Version { get; set; }
    }
}
