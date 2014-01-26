using ENode.Eventing;
using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a processor to process commands.
    /// </summary>
    public interface ICommandProcessor : IMessageProcessor<ICommandQueue, EventCommittingContext>
    {
    }
}
