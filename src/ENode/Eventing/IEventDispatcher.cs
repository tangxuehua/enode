using System.Collections.Generic;

namespace ENode.Eventing
{
    public interface IEventDispatcher
    {
        bool DispatchEvents(IEnumerable<IEvent> evnts);
        bool DispatchEvent(IEvent evnt);
    }
}
