using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    /// <summary>Represents a handler proxy.
    /// </summary>
    public interface IHandlerProxy
    {
        /// <summary>Get the inner handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerHandler();
    }
}
