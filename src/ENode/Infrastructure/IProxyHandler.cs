using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    /// <summary>Represents a proxy handler.
    /// </summary>
    public interface IProxyHandler
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        Task<AsyncTaskResult> Handle(object message);
        /// <summary>Get the inner handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerHandler();
    }
}
