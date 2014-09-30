namespace ENode.Infrastructure
{
    /// <summary>Represents a context environment for processing exception.
    /// </summary>
    public interface IExceptionProcessContext
    {
        /// <summary>Notify the given exception has been processed.
        /// </summary>
        /// <param name="exception">The processed exception.</param>
        void OnExceptionProcessed(IPublishableException exception);
    }
}
