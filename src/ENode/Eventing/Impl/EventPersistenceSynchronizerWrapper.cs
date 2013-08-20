namespace ENode.Eventing.Impl
{
    public class EventPersistenceSynchronizerWrapper<T> : IEventPersistenceSynchronizer where T : class, IEvent
    {
        private IEventPersistenceSynchronizer<T> _synchronizer;

        public EventPersistenceSynchronizerWrapper(IEventPersistenceSynchronizer<T> synchronizer)
        {
            _synchronizer = synchronizer;
        }

        public void OnBeforePersisting(IEvent evnt)
        {
            _synchronizer.OnBeforePersisting(evnt as T);
        }
        public void OnAfterPersisted(IEvent evnt)
        {
            _synchronizer.OnAfterPersisted(evnt as T);
        }
        public object GetInnerSynchronizer()
        {
            return _synchronizer;
        }
    }
}
