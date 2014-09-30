using ENode.Infrastructure;
namespace ENode.Eventing.Impl
{
    public class EventHandlerWrapper<TEvent> : MessageHandlerWrapper<IEventContext, TEvent, IEvent>, IEventHandler
        where TEvent : class, IEvent
    {
        public EventHandlerWrapper(IMessageHandler<IEventContext, TEvent> eventHandler) : base(eventHandler)
        {
        }
    }
}
