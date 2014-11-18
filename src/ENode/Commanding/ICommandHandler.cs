using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a command handler.
    /// </summary>
    public interface ICommandHandler : IProxyHandler
    {
        /// <summary>Handle the given command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        void Handle(ICommandContext context, object command);
    }
    /// <summary>Represents a command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
    {
        /// <summary>Handle the given command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        void Handle(ICommandContext context, TCommand command);
    }
}
