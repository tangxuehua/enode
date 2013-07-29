using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents an executor to execute an uncommitted event stream message.
    /// </summary>
    public interface IUncommittedEventExecutor : IMessageExecutor<EventStream>
    {
    }
}
