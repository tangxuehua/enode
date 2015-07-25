using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace ENode.MultiEventAndPriorityTests.Providers
{
    [Component]
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
