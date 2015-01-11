using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionDispatcher : AbstractDispatcher<IPublishableException, IExceptionHandler>
    {
        public DefaultExceptionDispatcher(
            ITypeCodeProvider<IPublishableException> messageTypeCodeProvider,
            ITypeCodeProvider<IExceptionHandler> handlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<IExceptionHandler> handlerProvider,
            ICommandService commandService,
            IRepository repository,
            IMessageHandleRecordStore messageHandleRecordStore,
            IMessageHandleRecordCache messageHandleRecordCache,
            ILoggerFactory loggerFactory)
            : base(
            messageTypeCodeProvider,
            handlerTypeCodeProvider,
            commandTypeCodeProvider,
            handlerProvider,
            commandService,
            repository,
            messageHandleRecordStore,
            messageHandleRecordCache,
            loggerFactory)
        {
        }

        protected override MessageHandleRecordType GetHandleRecordType(IPublishableException exception)
        {
            return MessageHandleRecordType.PublishableException;
        }
        protected override void HandleMessage(IPublishableException exception, IExceptionHandler exceptionHandler, IHandlingContext handlingContext)
        {
            exceptionHandler.Handle(handlingContext, exception);
        }
    }
}
