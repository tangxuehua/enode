using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an event publisher.
    /// </summary>
    public interface IEventPublisher : IMessagePublisher<EventStream>
    {
    }
}
