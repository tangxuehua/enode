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
        /// <summary>Represents the command result data.
        /// </summary>
        public string CommandResult { get; set; }
    }
}
