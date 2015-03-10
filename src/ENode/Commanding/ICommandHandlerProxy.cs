using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a command handler proxy.
    /// </summary>
    public interface ICommandHandlerProxy : IHandlerProxy
    {
        /// <summary>Handle the given command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        void Handle(ICommandContext context, ICommand command);
    }
}
