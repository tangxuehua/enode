using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Messaging.Impl
{
    public class DefaultMessageDispatcher : AbstractDispatcher<IMessage>
    {
        public DefaultMessageDispatcher(
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

        protected override MessageHandleRecordType GetHandleRecordType(IMessage message)
        {
            return MessageHandleRecordType.Message;
        }
    }
}
