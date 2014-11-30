using System;
using ENode.Eventing;

namespace ENode.EQueue
{
    [Serializable]
    public class EventMessage
    {
        public int EventTypeCode { get; set; }
        public string EventData { get; set; }
    }
}
