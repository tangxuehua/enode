using ENode.Infrastructure;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent : IMessage
    {
        /// <summary>Represents the id of the command which generates the domain event.
        /// </summary>
        string CommandId { get; set; }
        /// <summary>Represents the aggregate root string id of the sequence message.
        /// </summary>
        string AggregateRootStringId { get; set; }
        /// <summary>Represents the aggregate root type name of the sequence message.
        /// </summary>
        string AggregateRootTypeName { get; set; }
        /// <summary>Represents the main version of the sequence message.
        /// </summary>
        int Version { get; set; }
        /// <summary>Represents the event structure version.
        /// </summary>
        int SpecVersion { get; set; }
        /// <summary>Represents the child sequence for the current main version of message.
        /// </summary>
        int Sequence { get; set; }
    }
    /// <summary>Represents a domain event with generic type of aggregate root id.
    /// </summary>
    public interface IDomainEvent<TAggregateRootId> : IDomainEvent
    {
        TAggregateRootId AggregateRootId { get; set; }
    }
}
