using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        public override string GetTopic(IEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
