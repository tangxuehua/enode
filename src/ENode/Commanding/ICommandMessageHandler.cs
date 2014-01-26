using ENode.Eventing;
using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a command message handler interface.
    /// </summary>
    public interface ICommandMessageHandler : IMessageHandler<EventCommittingContext>
    {
    }
}
