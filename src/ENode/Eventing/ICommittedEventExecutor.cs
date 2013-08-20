using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents an executor to execute a committed event stream message.
    /// </summary>
    public interface ICommittedEventExecutor : IMessageExecutor<EventStream>
    {
    }
}
