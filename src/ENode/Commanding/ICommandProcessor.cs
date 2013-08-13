using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a processor to receive and process command.
    /// </summary>
    public interface ICommandProcessor : IMessageProcessor<ICommandQueue, ICommand>
    {
    }
}
