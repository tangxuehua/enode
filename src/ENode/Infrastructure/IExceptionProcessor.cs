namespace ENode.Infrastructure
{
    /// <summary>Represents an exception processor.
    /// </summary>
    public interface IExceptionProcessor
    {
        /// <summary>Process the given exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="context"></param>
        void Process(IPublishableException exception, IExceptionProcessContext context);
    }
}
