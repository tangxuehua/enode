using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a command queue.
    /// </summary>
    public interface ICommandQueue : IMessageQueue<ICommand>
    {
    }
}
