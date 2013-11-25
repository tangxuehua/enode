using System;
using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand : IMessagePayload
    {
        /// <summary>Represents the id of aggregate root which will be created or updated by the command.
        /// </summary>
        object AggregateRootId { get; }
        /// <summary>How many times the command should retry if meets concurrent exception.
        /// </summary>
        int RetryCount { get; }
    }
}
