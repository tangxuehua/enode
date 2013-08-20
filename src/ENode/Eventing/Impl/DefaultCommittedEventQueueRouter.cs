using System.Linq;
using System.Threading;

namespace ENode.Eventing.Impl
{
    public class DefaultCommittedEventQueueRouter : ICommittedEventQueueRouter
    {
        private ICommittedEventQueue[] _eventQueues;
        private int _index;

        public ICommittedEventQueue Route(EventStream stream)
        {
            if (_eventQueues == null)
            {
                _eventQueues = Configuration.Instance.GetCommitedEventQueues().ToArray();
            }

            if (_eventQueues.Length > 0)
            {
                return _eventQueues[(Interlocked.Increment(ref _index) - 1) % _eventQueues.Length];
            }

            return null;
        }
    }
}
