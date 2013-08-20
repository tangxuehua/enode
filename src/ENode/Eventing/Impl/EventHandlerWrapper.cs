namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventHandlerWrapper<T> : IEventHandler where T : class, IEvent
    {
        private readonly IEventHandler<T> _eventHandler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventHandler"></param>
        public EventHandlerWrapper(IEventHandler<T> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evnt"></param>
        public void Handle(object evnt)
        {
            _eventHandler.Handle(evnt as T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetInnerEventHandler()
        {
            return _eventHandler;
        }
    }
}
