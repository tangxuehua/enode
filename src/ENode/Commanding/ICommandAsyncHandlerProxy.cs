using System.Threading.Tasks;
using ECommon.IO;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an async handler proxy for command.
    /// </summary>
    public interface ICommandAsyncHandlerProxy : IObjectProxy
    {
        /// <summary>Handle the given application command async.
        /// </summary>
        /// <param name="command"></param>
        Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(ICommand command);
    }
}
