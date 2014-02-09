using System;

namespace ENode.EQueue
{
    /// <summary>Represents a message contains the information of the failed command.
    /// </summary>
    [Serializable]
    public class FailedCommandMessage
    {
        /// <summary>The unique identifier of the command.
        /// </summary>
        public Guid CommandId { get; set; }
        /// <summary>The aggregate root created or modified by the command.
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>The associated processId of the command.
        /// </summary>
        public string ProcessId { get; set; }
        /// <summary>The error message of the command result.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
