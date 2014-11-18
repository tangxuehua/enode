namespace ENode.Infrastructure
{
    /// <summary>Represents a handler proxy.
    /// </summary>
    public interface IProxyHandler
    {
        /// <summary>Get the inner handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerHandler();
    }
}
