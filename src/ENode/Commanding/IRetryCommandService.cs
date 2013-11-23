using ENode.Eventing;
using ENode.Infrastructure.Concurrent;

namespace ENode.Commanding
{
    /// <summary>Represents a command retry service.
    /// </summary>
    public interface IRetryCommandService
    {
        /// <summary>Retry the given command.
        /// </summary>
        void RetryCommand(CommandInfo commandInfo, EventStream eventStream, ConcurrentException concurrentException);
    }
}
