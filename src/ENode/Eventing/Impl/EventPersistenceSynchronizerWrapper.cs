namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventPersistenceSynchronizer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventPersistenceSynchronizerWrapper<T> : IEventPersistenceSynchronizer where T : class, IEvent
    {
        private readonly IEventPersistenceSynchronizer<T> _synchronizer;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="synchronizer"></param>
        public EventPersistenceSynchronizerWrapper(IEventPersistenceSynchronizer<T> synchronizer)
        {
            _synchronizer = synchronizer;
        }

        /// <summary>Executed before persisting the event.
        /// </summary>
        /// <param name="evnt"></param>
        public void OnBeforePersisting(IEvent evnt)
        {
            _synchronizer.OnBeforePersisting(evnt as T);
        }
        /// <summary>Executed after the event was persisted.
        /// </summary>
        /// <param name="evnt"></param>
        public void OnAfterPersisted(IEvent evnt)
        {
            _synchronizer.OnAfterPersisted(evnt as T);
        }
        /// <summary>Represents the inner generic IEventPersistenceSynchronizer.
        /// </summary>
        /// <returns></returns>
        public object GetInnerSynchronizer()
        {
            return _synchronizer;
        }
    }
}
