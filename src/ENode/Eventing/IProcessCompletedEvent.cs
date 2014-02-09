using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event which indicates a business process is completed.
    /// </summary>
    public interface IProcessCompletedEvent
    {
        string ProcessId { get; }
    }
}
