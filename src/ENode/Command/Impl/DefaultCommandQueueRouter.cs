using System.Linq;
using System.Threading;

namespace ENode.Commanding
{
    public class DefaultCommandQueueRouter : ICommandQueueRouter
    {
        private ICommandQueue[] _commandQueues;
        private int _index;

        public ICommandQueue Route(ICommand command)
        {
            if (_commandQueues == null)
            {
                _commandQueues = Configuration.Instance.GetCommandQueues().ToArray();
            }

            if (_commandQueues.Length > 0)
            {
                return _commandQueues[(Interlocked.Increment(ref _index) - 1) % _commandQueues.Length];
            }

            return null;
        }
    }
}
