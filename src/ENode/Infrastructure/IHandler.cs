using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    /// <summary>Represents a handler.
    /// </summary>
    public interface IHandler<in T> where T : class
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        Task<AsyncTaskResult> Handle(T message);
    }
}
