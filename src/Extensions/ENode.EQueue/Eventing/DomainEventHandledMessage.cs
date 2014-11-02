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
        /// <summary>Represents the extension information of the domain event.
        /// </summary>
        public IDictionary<string, string> Items { get; set; }
    }
}
