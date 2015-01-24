using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventDispatcher : AbstractDispatcher<IEvent, IEventHandler>
    {
        public DefaultEventDispatcher(
            ITypeCodeProvider<IEvent> messageTypeCodeProvider,
            ITypeCodeProvider<IEventHandler> handlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<IEventHandler> handlerProvider,
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

        protected override MessageHandleRecordType GetHandleRecordType(IEvent evnt)
        {
            return evnt is IDomainEvent ? MessageHandleRecordType.DomainEvent : MessageHandleRecordType.Event;
        }
        protected override void HandleMessage(IEvent evnt, IEventHandler eventHandler, IHandlingContext handlingContext)
        {
            eventHandler.Handle(handlingContext, evnt);
        }
        protected override void OnMessageHandleRecordCreated(IEvent evnt, MessageHandleRecord record)
        {
            base.OnMessageHandleRecordCreated(evnt, record);
            var domainEvent = evnt as IDomainEvent;
            if (domainEvent != null)
            {
                record.AggregateRootId = domainEvent.AggregateRootId;
                record.AggregateRootVersion = domainEvent.Version;
            }
        }
    }
}
