using System.Threading.Tasks;
using ECommon.IO;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a command store to store all the command async handle records.
    /// </summary>
    public interface ICommandStore
    {
        /// <summary>Add the given handled command to the command store async.
        /// </summary>
        Task<AsyncTaskResult<CommandAddResult>> AddAsync(HandledCommand handledCommand);
        /// <summary>Get a handled command by commandId async.
        /// </summary>
        Task<AsyncTaskResult<HandledCommand>> GetAsync(string commandId);
    }
}
