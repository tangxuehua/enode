using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message publisher.
    /// </summary>
    public interface IMessagePublisher<TMessage> where TMessage : class, IMessage
    {
        /// <summary>Publish the given message to all the subscribers async.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<AsyncTaskResult> PublishAsync(TMessage message);
    }
}
