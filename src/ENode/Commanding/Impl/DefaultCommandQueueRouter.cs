using System.Linq;
using System.Threading;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandQueueRouter.
    /// </summary>
    public class DefaultCommandQueueRouter : ICommandQueueRouter
    {
        private ICommandQueue[] _commandQueues;
        private int _index;

        /// <summary>Route the given command to a specified command queue.
        /// </summary>
        /// <param name="command">The command</param>
        /// <returns>Returns the routed command queue.</returns>
        public ICommandQueue Route(ICommand command)
        {
            if (_commandQueues == null)
            {
                _commandQueues = Configuration.Instance.GetCommandQueues().ToArray();
            }

            return _commandQueues.Length > 0 ? _commandQueues[(Interlocked.Increment(ref _index) - 1) % _commandQueues.Length] : null;
        }
    }
}
