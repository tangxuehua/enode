using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class ExceptionHandlerWrapper<TException> : MessageHandlerWrapper<IExceptionHandlingContext, TException, IException>, IExceptionHandler
        where TException : class, IException
    {
        public ExceptionHandlerWrapper(IMessageHandler<IExceptionHandlingContext, TException> exceptionHandler) : base(exceptionHandler)
        {
        }
    }
}
