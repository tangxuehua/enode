using System;
using ENode.Eventing;

namespace ENode.EQueue
{
    [Serializable]
    public class EventMessage
    {
        public IEvent Event { get; set; }
    }
}
