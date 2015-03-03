using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionDispatcher : AbstractDispatcher<IPublishableException>
    {
        public DefaultExceptionDispatcher(
            ITypeCodeProvider typeCodeProvider,
            IHandlerProvider handlerProvider,
            ICommandService commandService,
            IRepository repository,
            IMessageHandleRecordStore messageHandleRecordStore,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
            : base(
            typeCodeProvider,
            handlerProvider,
            commandService,
            repository,
            messageHandleRecordStore,
            ioHelper,
            loggerFactory)
        {
        }

        protected override MessageHandleRecordType GetHandleRecordType(IPublishableException exception)
        {
            return MessageHandleRecordType.PublishableException;
        }
    }
}
