using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    /// <summary>Represents a handler proxy.
    /// </summary>
    public interface IObjectProxy
    {
        /// <summary>Get the inner object.
        /// </summary>
        /// <returns></returns>
        object GetInnerObject();
    }
}
