using System;
using ENode.Eventing.Impl;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>An internal class to contains the context information when processing an event stream.
    /// </summary>
    public class MessageContext
    {
        public string QueueName { get; set; }
        public EventProcessStatus ProcessStatus { get; set; }
    }
}
