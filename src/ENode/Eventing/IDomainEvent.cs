using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent : ISequenceMessage
    {
    }
    /// <summary>Represents a domain event with generic type of aggregate root id.
    /// </summary>
    public interface IDomainEvent<TAggregateRootId> : IDomainEvent
    {
        TAggregateRootId AggregateRootId { get; set; }
    }
}
