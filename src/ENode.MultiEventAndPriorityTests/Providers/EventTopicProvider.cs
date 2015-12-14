using ENode.EQueue;
using ENode.Eventing;

namespace ENode.MultiEventAndPriorityTests.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
