using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents an handler to handle the uncommitted event stream message.
    /// </summary>
    public interface IUncommittedEventMessageHandler : IMessageHandler<EventStream>
    {
    }
}
