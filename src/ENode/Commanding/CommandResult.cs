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

        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(CommandStatus status, string commandId, string aggregateRootId, string exceptionTypeName, string errorMessage)
        {
            Status = status;
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            ExceptionTypeName = exceptionTypeName;
            ErrorMessage = errorMessage;
        }

        /// <summary>Overrides to return the command result info.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[CommandId={0},Status={1},AggregateRootId={2},ExceptionTypeName={3},ErrorMessage={4}]",
                CommandId,
                Status,
                AggregateRootId,
                ExceptionTypeName,
                ErrorMessage);
        }
    }
    /// <summary>Represents the command result status enum.
    /// </summary>
    public enum CommandStatus
    {
        None = 0,
        Success = 1,
        NothingChanged = 2,
        Failed = 3
    }
}
