using System.Linq;
using System.Threading;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultCommittedEventQueueRouter : ICommittedEventQueueRouter
    {
        private ICommittedEventQueue[] _eventQueues;
        private int _index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public ICommittedEventQueue Route(EventStream stream)
        {
            if (_eventQueues == null)
            {
                _eventQueues = Configuration.Instance.GetCommitedEventQueues().ToArray();
            }

            return _eventQueues.Length > 0 ? _eventQueues[(Interlocked.Increment(ref _index) - 1) % _eventQueues.Length] : null;
        }
    }
}
