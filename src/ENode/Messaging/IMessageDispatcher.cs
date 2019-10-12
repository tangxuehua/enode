using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENode.Messaging
{
    /// <summary>Represents a message dispatcher.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>Dispatch the given message async.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task DispatchMessageAsync(IMessage message);
        /// <summary>Dispatch the given messages async.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task DispatchMessagesAsync(IEnumerable<IMessage> messages);
    }
}
