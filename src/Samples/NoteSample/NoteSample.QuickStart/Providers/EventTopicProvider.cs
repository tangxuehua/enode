using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.QuickStart
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return Constants.EventTopic;
        }
    }
}
