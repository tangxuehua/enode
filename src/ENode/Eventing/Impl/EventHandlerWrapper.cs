namespace ENode.Eventing.Impl
{
    public class EventHandlerWrapper<T> : IEventHandler where T : class, IEvent
    {
        private IEventHandler<T> _eventHandler;

        public EventHandlerWrapper(IEventHandler<T> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public void Handle(object evnt)
        {
            _eventHandler.Handle(evnt as T);
        }
        public object GetInnerEventHandler()
        {
            return _eventHandler;
        }
    }
}
