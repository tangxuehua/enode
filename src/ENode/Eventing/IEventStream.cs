using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    public interface IEventStream
    {
        string CommandId { get; }
        string ProcessId { get; }
        IEnumerable<IEvent> Events { get; }
        IDictionary<string, string> Items { get; }
    }
}
