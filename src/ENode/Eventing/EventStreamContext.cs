using System;
using ENode.Eventing.Impl;
using ENode.Infrastructure.Concurrent;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>An internal class to contains the context information when processing an event stream.
    /// </summary>
    internal class EventStreamContext
    {
        public EventStream EventStream { get; set; }
        public EventProcessStatus ProcessStatus { get; set; }
    }
}
