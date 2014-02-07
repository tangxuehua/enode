using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command result.
    /// </summary>
    [Serializable]
    public class CommandResult
    {
        /// <summary>The status of the command result.
        /// </summary>
        public CommandStatus Status { get; private set; }
        /// <summary>The unique identifier of the command.
        /// </summary>
        public Guid CommandId { get; private set; }
        /// <summary>The aggregate root created or modified by the command.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>Represents whether the command result in some domain events which make a business process completed.
        /// </summary>
        public bool IsProcessCompletedEventPublished { get; private set; }
        /// <summary>The error message of the command result.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        public CommandResult(ICommand command) : this(command, false) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(ICommand command, bool isProcessCompletedEventPublished)
        {
            Status = CommandStatus.Success;
            CommandId = command.Id;
            AggregateRootId = command.AggregateRootId;
            IsProcessCompletedEventPublished = isProcessCompletedEventPublished;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(ICommand command, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentNullException("errorMessage");
            }
            Status = CommandStatus.Failed;
            CommandId = command.Id;
            AggregateRootId = command.AggregateRootId;
            ErrorMessage = errorMessage;
            IsProcessCompletedEventPublished = false;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(ICommand command, Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            Status = CommandStatus.Failed;
            CommandId = command.Id;
            AggregateRootId = command.AggregateRootId;
            ErrorMessage = exception.Message;
            IsProcessCompletedEventPublished = false;
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
