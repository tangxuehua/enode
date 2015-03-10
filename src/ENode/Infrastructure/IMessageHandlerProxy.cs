using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message handler proxy.
    /// </summary>
    public interface IMessageHandlerProxy : IHandlerProxy
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        Task<AsyncTaskResult> HandleAsync(IMessage message);
    }
}
