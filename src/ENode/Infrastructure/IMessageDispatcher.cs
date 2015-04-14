using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message dispatcher.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>Dispatch the given message async.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<AsyncTaskResult> DispatchMessageAsync(IMessage message);
        /// <summary>Dispatch the given messages async.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task<AsyncTaskResult> DispatchMessagesAsync(IEnumerable<IMessage> messages);
    }
}
