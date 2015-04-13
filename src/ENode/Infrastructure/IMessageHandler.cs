using System.Threading.Tasks;
using ECommon.Retring;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler<in T> where T : class, IMessage
    {
        /// <summary>Handle the given message async.
        /// </summary>
        /// <param name="message"></param>
        Task<AsyncTaskResult> HandleAsync(T message);
    }
}
