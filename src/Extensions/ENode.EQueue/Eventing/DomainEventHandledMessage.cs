using System;

namespace ENode.EQueue
{
    /// <summary>Represents a message contains the information of the handled domain event.
    /// </summary>
    [Serializable]
    public class DomainEventHandledMessage
    {
        /// <summary>The unique identifier of the command.
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>The aggregate root created or modified by the command.
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>Indicates whether the domain event represent a business process is completed.
        /// </summary>
        public bool IsProcessCompletedEvent { get; set; }
        /// <summary>If the IsProcessCompletedEvent property is true, the value of this property represents the id of the process; otherwise, the value is null.
        /// </summary>
        public string ProcessId { get; set; }
    }
}
