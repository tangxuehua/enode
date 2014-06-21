using System;

namespace ENode.EQueue
{
    /// <summary>Represents a message contains the information of the handled domain event.
    /// </summary>
    [Serializable]
    public class DomainEventHandledMessage
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>Represents the aggregate root created or modified by the command.
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>Represents whether the domain event indicates a business process is completed.
        /// </summary>
        public bool IsProcessCompleted { get; set; }
        /// <summary>Represents whether the process is success.
        /// </summary>
        public bool IsProcessSuccess { get; set; }
        /// <summary>Represents the error code if the process is not success.
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>If the IsProcessCompletedEvent property is true, the value of this property represents the id of the process; otherwise, the value is null.
        /// </summary>
        public string ProcessId { get; set; }
    }
}
