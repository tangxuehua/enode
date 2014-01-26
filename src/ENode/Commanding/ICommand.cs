using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        Guid Id { get; }
        /// <summary>Represents the id of aggregate root which will be created or updated by the command.
        /// </summary>
        object AggregateRootId { get; }
        /// <summary>How many times the command should retry if meets concurrent exception.
        /// </summary>
        int RetryCount { get; }
    }
}
