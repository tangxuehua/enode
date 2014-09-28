using ENode.EQueue;
using ENode.Eventing;

namespace UniquenessConstraintSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        public override string GetTopic(IEvent source)
        {
            return "SectionEventTopic";
        }
    }
}
