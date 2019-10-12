using System.Threading.Tasks;

namespace ENode.Messaging
{
    /// <summary>Represents a message publisher.
    /// </summary>
    public interface IMessagePublisher<TMessage> where TMessage : class, IMessage
    {
        /// <summary>Publish the given message to all the subscribers async.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishAsync(TMessage message);
    }
}
