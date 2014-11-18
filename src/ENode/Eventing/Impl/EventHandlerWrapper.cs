using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class EventHandlerWrapper<TEvent> : IEventHandler where TEvent : class, IEvent
    {
        private readonly IEventHandler<TEvent> _eventHandler;

        public EventHandlerWrapper(IEventHandler<TEvent> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public void Handle(IHandlingContext context, object evnt)
        {
            _eventHandler.Handle(context, evnt as TEvent);
        }
        public object GetInnerHandler()
        {
            return _eventHandler;
        }
    }
}
