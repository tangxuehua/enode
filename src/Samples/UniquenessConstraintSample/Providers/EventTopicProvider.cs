using ENode.EQueue;
using ENode.Eventing;

namespace UniquenessConstraintSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "SectionEventTopic";
        }
    }
}
