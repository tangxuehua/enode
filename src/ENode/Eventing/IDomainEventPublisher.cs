using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a publisher to publish the committed domain event stream to all the event handlers.
    /// </summary>
    public interface IDomainEventPublisher
    {
        void PublishEvent(DomainEventStream eventStream, IDictionary<string, string> contextItems);
    }
}
