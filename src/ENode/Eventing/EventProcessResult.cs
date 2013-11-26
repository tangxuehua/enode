using System;

namespace ENode.Eventing.Impl
{
    /// <summary>Represents the event process result.
    /// </summary>
    public class EventProcessResult
    {
        public Guid EventStreamId { get; set; }
        public EventProcessStatus Status { get; set; }
    }

    /// <summary>Event process status enum.
    /// </summary>
    public enum EventProcessStatus
    {
        Success = 1,
        SynchronizerConcurrentException,
        SynchronizerFailed,
        ConcurrentException,
        PublishFailed,
        Failed
    }
}
