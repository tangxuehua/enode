using System.Collections.Generic;

namespace ENode.Eventing
{
    public interface IPublishEventService
    {
        void PublishEvent(IDictionary<string, object> contextItems, EventStream eventStream);
    }
}
