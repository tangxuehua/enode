using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Messaging.Impl
{
    public class DefaultMessageDispatcher : AbstractDispatcher<IMessage, IMessageHandler>
    {
        public DefaultMessageDispatcher(
            ITypeCodeProvider<IMessage> messageTypeCodeProvider,
            ITypeCodeProvider<IMessageHandler> handlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<IMessageHandler> handlerProvider,
            ICommandService commandService,
            IRepository repository,
            IMessageHandleRecordStore messageHandleRecordStore,
            IMessageHandleRecordCache messageHandleRecordCache,
            IOHelper ioHelper,
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
            ioHelper,
            loggerFactory)
        {
        }

        protected override MessageHandleRecordType GetHandleRecordType(IMessage message)
        {
            return MessageHandleRecordType.Message;
        }
        protected override void HandleMessage(IMessage message, IMessageHandler messageHandler, IHandlingContext handlingContext)
        {
            messageHandler.Handle(handlingContext, message);
        }
    }
}
