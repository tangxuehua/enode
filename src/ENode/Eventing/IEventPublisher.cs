using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents an event publisher.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>Publish the given events to all the event handlers.
        /// </summary>
        /// <param name="eventStream"></param>
        /// <param name="contextItems"></param>
        void PublishEvent(EventStream eventStream, IDictionary<string, string> contextItems);
    }
}
