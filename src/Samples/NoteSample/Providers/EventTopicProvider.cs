using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<EventStream>
    {
        public override string GetTopic(EventStream eventStream)
        {
            return "NoteEventTopic";
        }
    }
}
