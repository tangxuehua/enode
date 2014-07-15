using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
