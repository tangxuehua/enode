namespace ENode.Infrastructure
{
    /// <summary>Represents an exception publisher.
    /// </summary>
    public interface IExceptionPublisher
    {
        /// <summary>Publish the given exception to all the exception handlers.
        /// </summary>
        /// <param name="exception"></param>
        void PublishException(IPublishableException exception);
    }
}
