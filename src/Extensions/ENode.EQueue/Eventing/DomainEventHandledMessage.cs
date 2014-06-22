using System;
using System.Collections.Generic;

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
        /// <summary>Represents the unique identifier of the business process.
        /// </summary>
        public string ProcessId { get; set; }
        /// <summary>Represents whether the domain event indicates a business process is completed.
        /// </summary>
        public bool IsProcessCompleted { get; set; }
        /// <summary>Represents whether the process is success.
        /// </summary>
        public bool IsProcessSuccess { get; set; }
        /// <summary>Represents the error code if the process is not success.
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>Represents the extension information of the domain event.
        /// </summary>
        public IDictionary<string, string> Items { get; set; }
    }
}
