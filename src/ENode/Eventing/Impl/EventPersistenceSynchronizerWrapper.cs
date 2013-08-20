namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventPersistenceSynchronizerWrapper<T> : IEventPersistenceSynchronizer where T : class, IEvent
    {
        private readonly IEventPersistenceSynchronizer<T> _synchronizer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="synchronizer"></param>
        public EventPersistenceSynchronizerWrapper(IEventPersistenceSynchronizer<T> synchronizer)
        {
            _synchronizer = synchronizer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evnt"></param>
        public void OnBeforePersisting(IEvent evnt)
        {
            _synchronizer.OnBeforePersisting(evnt as T);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evnt"></param>
        public void OnAfterPersisted(IEvent evnt)
        {
            _synchronizer.OnAfterPersisted(evnt as T);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetInnerSynchronizer()
        {
            return _synchronizer;
        }
    }
}
