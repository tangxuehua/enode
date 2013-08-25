using System.Linq;
using System.Threading;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IUncommittedEventQueueRouter.
    /// </summary>
    public class DefaultUncommittedEventQueueRouter : IUncommittedEventQueueRouter
    {
        private IUncommittedEventQueue[] _eventQueues;
        private int _index;

        /// <summary>Route a available uncommitted event queue for the given event stream message.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public IUncommittedEventQueue Route(EventStream stream)
        {
            if (_eventQueues == null)
            {
                _eventQueues = Configuration.Instance.GetUncommitedEventQueues().ToArray();
            }

            return _eventQueues.Length > 0 ? _eventQueues[(Interlocked.Increment(ref _index) - 1) % _eventQueues.Length] : null;
        }
    }
}
