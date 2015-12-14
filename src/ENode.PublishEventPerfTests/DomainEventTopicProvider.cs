using ENode.EQueue;
using ENode.Eventing;

namespace ENode.PublishEventPerfTests
{
    public class DomainEventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
