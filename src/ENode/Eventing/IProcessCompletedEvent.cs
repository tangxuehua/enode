using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event which indicates a business process is completed.
    /// </summary>
    public interface IProcessCompletedEvent
    {
        /// <summary>Represents whether the process is success.
        /// </summary>
        bool IsSuccess { get; }
        /// <summary>Represents the error code if the process is not success.
        /// </summary>
        int ErrorCode { get; }
    }
}
