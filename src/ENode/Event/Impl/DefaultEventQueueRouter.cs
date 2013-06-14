using System.Linq;
using System.Threading;

namespace ENode.Eventing
{
    public class DefaultEventQueueRouter : IEventQueueRouter
    {
        private IEventQueue[] _eventQueues;
        private int _index;

        public IEventQueue Route(EventStream stream)
        {
            if (_eventQueues == null)
            {
                _eventQueues = Configuration.Instance.GetEventQueues().ToArray();
            }

            if (_eventQueues.Length > 0)
            {
                return _eventQueues[(Interlocked.Increment(ref _index) - 1) % _eventQueues.Length];
            }

            return null;
        }
    }
}
