namespace ENode.Infrastructure
{
    /// <summary>Represents a event handler.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>Handle the given exception.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void Handle(IExceptionContext context, object exception);
        /// <summary>Get the inner event handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerExceptionHandler();
    }
    /// <summary>Represents a event handler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IExceptionHandler<in TException> where TException : class, IPublishableException
    {
        /// <summary>Handle the given exception.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void Handle(IExceptionContext context, TException exception);
    }
}
