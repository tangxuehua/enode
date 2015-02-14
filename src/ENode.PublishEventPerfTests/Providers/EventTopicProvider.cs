using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace ENode.PublishEventPerfTests
{
    [Component]
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        public override string GetTopic(IEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
