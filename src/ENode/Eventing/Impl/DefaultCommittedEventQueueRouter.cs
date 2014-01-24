using System.Linq;
using System.Threading;
using ENode.Configurations;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of ICommittedEventQueueRouter.
    /// </summary>
    public class DefaultCommittedEventQueueRouter : ICommittedEventQueueRouter
    {
        private ICommittedEventQueue[] _eventQueues;
        private int _index;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public ICommittedEventQueue Route(EventStream stream)
        {
            if (_eventQueues == null)
            {
                _eventQueues = ENodeConfiguration.Instance.GetCommitedEventQueues().ToArray();
            }

            return _eventQueues.Length > 0 ? _eventQueues[(Interlocked.Increment(ref _index) - 1) % _eventQueues.Length] : null;
        }
    }
}
