using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class ExceptionHandlerWrapper<TException> : IExceptionHandler where TException : class, IPublishableException
    {
        private readonly IExceptionHandler<TException> _exceptionHandler;

        public ExceptionHandlerWrapper(IExceptionHandler<TException> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        public void Handle(IExceptionHandlingContext context, object exception)
        {
            _exceptionHandler.Handle(context, exception as TException);
        }
        public object GetInnerHandler()
        {
            return _exceptionHandler;
        }
    }
}
