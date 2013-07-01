using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a command executor interface.
    /// </summary>
    public interface ICommandExecutor : IMessageExecutor<ICommand>
    {
    }
}
