namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventHandler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class EventHandlerWrapper<TEvent> : MessageHandlerWrapper<IEventContext, TEvent, IEvent>, IEventHandler
        where TEvent : class, IEvent
    {
        public EventHandlerWrapper(IMessageHandler<IEventContext, TEvent> eventHandler) : base(eventHandler)
        {
        }
    }
}
