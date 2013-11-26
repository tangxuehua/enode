using ENode.Eventing.Impl;

namespace ENode.Eventing
{
    /// <summary>An internal class to contains the context information when processing an event stream.
    /// </summary>
    internal class EventProcessingContext
    {
        public EventStream EventStream { get; set; }
        public EventProcessStatus ProcessStatus { get; set; }
    }
}
