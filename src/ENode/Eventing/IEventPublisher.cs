using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a publisher to publish the committed event stream to all the event handlers.
    /// </summary>
    public interface IEventPublisher
    {
        void PublishEvent(IDictionary<string, string> contextItems, EventStream eventStream);
    }
}
