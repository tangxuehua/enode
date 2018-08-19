using ENode.Infrastructure;
using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a command handler proxy.
    /// </summary>
    public interface ICommandHandlerProxy : IObjectProxy
    {
        /// <summary>Handle the given command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        Task HandleAsync(ICommandContext context, ICommand command);
    }
}
