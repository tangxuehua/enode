using ENode.Messaging;

namespace ENode.Commanding {
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand : IMessage {
        int MillisecondsTimeout { get; }
        int RetryCount { get; }
    }
}
