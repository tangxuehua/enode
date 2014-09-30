using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event publisher.
    /// </summary>
    public interface IDomainEventPublisher : IMessagePublisher<DomainEventStream>
    {
    }
}
