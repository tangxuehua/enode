using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a command handler interface.
    /// </summary>
    public interface ICommandHandler : IMessageHandler
    {
    }
    /// <summary>Represents a command handler interface.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> : IMessageHandler<ICommandContext, TCommand> where TCommand : class, ICommand
    {
    }
}
