using ENode.Infrastructure;

namespace ENode.Exceptions
{
    /// <summary>Represents an exception handler.
    /// </summary>
    public interface IExceptionHandler : IProxyHandler
    {
        /// <summary>Handle the given exception.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void Handle(IExceptionHandlingContext context, object exception);
    }
    /// <summary>Represents an exception handler.
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    public interface IExceptionHandler<in TException> where TException : class, IPublishableException
    {
        /// <summary>Handle the given exception.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void Handle(IExceptionHandlingContext context, TException exception);
    }
}
