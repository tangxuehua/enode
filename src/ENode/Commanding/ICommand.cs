using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand : IMessage
    {
        /// <summary>Represents the id of aggregate root which will be created or updated by the current command.
        /// </summary>
        object AggregateRootId { get; }
        /// <summary>Command executing waiting milliseconds.
        /// </summary>
        int MillisecondsTimeout { get; }
        /// <summary>How many times the command should retry.
        /// </summary>
        int RetryCount { get; }
    }
}
