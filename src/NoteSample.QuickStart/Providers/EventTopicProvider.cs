using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.QuickStart.Providers
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
