using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.EQueue
{
    [Serializable]
    public class EventStreamMessage
    {
        public string CommandId { get; set; }
        public IDictionary<int, string> Events { get; set; }
        public IDictionary<string, string> Items { get; set; }
    }
}
