using System;

namespace ENode.Eventing.Impl
{
    /// <summary>Represents the event process result.
    /// </summary>
    public class EventProcessResult
    {
        public Guid EventStreamId { get; set; }
        public EventProcessStatus ProcessStatus { get; set; }
    }
}
