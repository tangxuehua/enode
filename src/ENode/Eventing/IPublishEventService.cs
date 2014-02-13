using System.Collections.Generic;

namespace ENode.Eventing
{
    public interface IPublishEventService
    {
        void PublishEvent(IDictionary<string, string> contextItems, EventStream eventStream);
    }
}
