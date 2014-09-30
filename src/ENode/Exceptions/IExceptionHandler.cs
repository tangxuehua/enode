using ENode.Infrastructure;

namespace ENode.Exceptions
{
    /// <summary>Represents an exception handler.
    /// </summary>
    public interface IExceptionHandler : IMessageHandler
    {
    }
    /// <summary>Represents an exception handler.
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    public interface IExceptionHandler<in TException> : IMessageHandler<IExceptionHandlingContext, TException>
        where TException : class, IException
    {
    }
}
