using ENode.EQueue;
using ENode.Eventing;

namespace UniquenessConstraintSample.Providers
{
    public class EventTopicProvider : IEventTopicProvider
    {
        public string GetTopic(EventStream eventStream)
        {
            return "SectionEventTopic";
        }
    }
}
