using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class ExceptionHandlerWrapper<TException> : MessageHandlerWrapper<IExceptionHandlingContext, TException, IPublishableException>, IExceptionHandler
        where TException : class, IPublishableException
    {
        public ExceptionHandlerWrapper(IMessageHandler<IExceptionHandlingContext, TException> exceptionHandler) : base(exceptionHandler)
        {
        }
    }
}
