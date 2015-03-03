using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventDispatcher : AbstractDispatcher<IEvent>
    {
        public DefaultEventDispatcher(
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

        protected override MessageHandleRecordType GetHandleRecordType(IEvent evnt)
        {
            return evnt is IDomainEvent ? MessageHandleRecordType.DomainEvent : MessageHandleRecordType.Event;
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
