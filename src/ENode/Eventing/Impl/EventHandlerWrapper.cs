namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventHandler.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventHandlerWrapper<T> : IEventHandler where T : class, IEvent
    {
        private readonly IEventHandler<T> _eventHandler;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventHandler"></param>
        public EventHandlerWrapper(IEventHandler<T> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        /// <summary>Handle the given event.
        /// </summary>
        /// <param name="evnt"></param>
        public void Handle(object evnt)
        {
            _eventHandler.Handle(evnt as T);
        }

        /// <summary>Get the inner event handler.
        /// </summary>
        /// <returns></returns>
        public object GetInnerEventHandler()
        {
            return _eventHandler;
        }
    }
}
