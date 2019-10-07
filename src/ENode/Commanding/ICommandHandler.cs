using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents generic command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
    {
        /// <summary>Handle the given aggregate command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        Task HandleAsync(ICommandContext context, TCommand command);
    }
}
