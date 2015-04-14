using System.Threading.Tasks;
using ECommon.IO;

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
