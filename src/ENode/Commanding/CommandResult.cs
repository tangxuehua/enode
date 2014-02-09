using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command result.
    /// </summary>
    [Serializable]
    public class CommandResult
    {
        /// <summary>The status of the command.
        /// </summary>
        public CommandStatus Status { get; private set; }
        /// <summary>The unique identifier of the command.
        /// </summary>
        public Guid CommandId { get; private set; }
        /// <summary>The aggregate root created or modified by the command.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The error message of the command result.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(Guid commandId, string aggregateRootId)
        {
            Status = CommandStatus.Success;
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
        }

        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(Guid commandId, string aggregateRootId, Exception exception) : this(commandId, aggregateRootId, exception.Message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(Guid commandId, string aggregateRootId, string errorMessage)
        {
            Status = CommandStatus.Failed;
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            ErrorMessage = errorMessage;
        }
    }
    /// <summary>Represents the command processing status.
    /// </summary>
    public enum CommandStatus
    {
        Success = 1,
        Failed,
    }
}
