using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a publisher to publish the committed event stream to all the event handlers.
    /// </summary>
    public interface IEventPublisher
    {
        void PublishEvent(IDictionary<string, object> contextItems, EventStream eventStream);
    }
}
