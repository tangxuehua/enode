using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.Providers
{
    public class EventTopicProvider : IEventTopicProvider
    {
        public string GetTopic(EventStream eventStream)
        {
            return "NoteEventTopic";
        }
    }
}
