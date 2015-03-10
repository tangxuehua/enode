using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent : ISequenceMessage
    {
    }
}
