using ENode.EQueue;
using ENode.Eventing;

namespace UniquenessConstraintSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<EventStream>
    {
        public override string GetTopic(EventStream eventStream)
        {
            return "SectionEventTopic";
        }
    }
}
