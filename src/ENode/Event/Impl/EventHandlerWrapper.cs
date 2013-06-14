namespace ENode.Eventing
{
    public class EventHandlerWrapper<T> : IEventHandlerWrapper, IEventHandler where T : class, IEvent
    {
        private IEventHandler<T> _eventHandler;

        public EventHandlerWrapper(IEventHandler<T> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public object GetInnerEventHandler()
        {
            return _eventHandler;
        }
        public void Handle(object evnt)
        {
            _eventHandler.Handle(evnt as T);
        }
    }
}
