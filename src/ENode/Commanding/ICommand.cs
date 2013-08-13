using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand : IMessage
    {
        /// <summary>Command executing waiting milliseconds.
        /// </summary>
        int MillisecondsTimeout { get; }
        /// <summary>How many times the command should retry.
        /// </summary>
        int RetryCount { get; }
    }
}
