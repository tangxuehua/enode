using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        string Id { get; set; }
        /// <summary>Represents the id of aggregate root which is created or updated by the command.
        /// </summary>
        string AggregateRootId { get; }
        /// <summary>How many times the command should retry if meets concurrent exception.
        /// </summary>
        int RetryCount { get; }
        /// <summary>Represents a key of the command.
        /// <remarks>
        /// The framework will use the domain event id, event handler type code, command type code,
        /// and this key to build a unique id for the current command. The default key is the aggregate root id.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        object GetKey();
    }
}
