using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command send result.
    /// </summary>
    [Serializable]
    public class CommandSendResult
    {
        /// <summary>Represents the send status of the command.
        /// </summary>
        public CommandSendStatus Status { get; private set; }
        /// <summary>Represents the error message if send the command is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public CommandSendResult(CommandSendStatus status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }
    }
    /// <summary>Represents the command send result status enum.
    /// </summary>
    public enum CommandSendStatus
    {
        Success = 1,
        Failed,
    }
}
