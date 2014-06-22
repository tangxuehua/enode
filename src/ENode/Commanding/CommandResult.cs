using System;
using System.Collections.Generic;

namespace ENode.Commanding
{
    /// <summary>Represents a command result.
    /// </summary>
    [Serializable]
    public class CommandResult
    {
        /// <summary>Represents the result status of the command.
        /// </summary>
        public CommandStatus Status { get; private set; }
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public string CommandId { get; private set; }
        /// <summary>Represents the aggregate root id associated with the command.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>Represents the exception type name if the command has any exception.
        /// </summary>
        public string ExceptionTypeName { get; private set; }
        /// <summary>Represents the error message if the command is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>Represents the extension information of the command result.
        /// </summary>
        public IDictionary<string, string> Items { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(CommandStatus status, string commandId, string aggregateRootId, string exceptionTypeName, string errorMessage, IDictionary<string, string> items)
        {
            Status = status;
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            ExceptionTypeName = exceptionTypeName;
            ErrorMessage = errorMessage;
            Items = items ?? new Dictionary<string, string>();
        }
    }
    /// <summary>Represents the command result status enum.
    /// </summary>
    public enum CommandStatus
    {
        Success = 1,
        NothingChanged = 2,
        Failed = 3,
    }
}
