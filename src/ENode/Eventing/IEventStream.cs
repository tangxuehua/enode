using System.Collections.Generic;

namespace ENode.Eventing
{
    public interface IEventStream
    {
        string CommandId { get; }
        IEnumerable<IEvent> Events { get; }
        IDictionary<string, string> Items { get; }
    }
}
