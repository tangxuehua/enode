using ENode.EQueue;
using ENode.Eventing;

namespace ENode.Tests
{
    public class DomainEventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "EventTopic";
        }
    }
}
