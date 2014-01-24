using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command result.
    /// </summary>
    public class CommandResult
    {
        /// <summary>Represents a success command result.
        /// </summary>
        public static readonly CommandResult Success = new CommandResult();

        /// <summary>The status of the command result.
        /// </summary>
        public CommandResultStatus Status { get; private set; }
        /// <summary>The error message of the command result.
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>The exception of the command result.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        public CommandResult()
        {
            Status = CommandResultStatus.Success;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentNullException("errorMessage");
            }
            Status = CommandResultStatus.Failed;
            ErrorMessage = errorMessage;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            Status = CommandResultStatus.Failed;
            ErrorMessage = exception.Message;
            Exception = exception;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandResult(string errorMessage, Exception exception)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentNullException("errorMessage");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            Status = CommandResultStatus.Failed;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }
    /// <summary>Represents the command result status enum.
    /// </summary>
    public enum CommandResultStatus
    {
        Success = 1,
        Failed,
    }
}
