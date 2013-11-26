using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents an handler to handle the committed event stream message.
    /// </summary>
    public interface ICommittedEventMessageHandler : IMessageHandler<EventStream>
    {
    }
}
