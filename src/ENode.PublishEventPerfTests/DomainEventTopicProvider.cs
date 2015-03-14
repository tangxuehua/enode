using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace ENode.PublishEventPerfTests
{
    [Component]
    public class DomainEventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
