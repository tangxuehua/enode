using System.Threading.Tasks;
using ECommon.IO;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an async handler for command.
    /// </summary>
    public interface ICommandAsyncHandler
    {
    }
    /// <summary>Represents an generic async handler for command.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandAsyncHandler<in TCommand> : ICommandAsyncHandler where TCommand : class, ICommand
    {
        /// <summary>Handle the given command async.
        /// </summary>
        /// <param name="command"></param>
        Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(TCommand command);
    }
}
