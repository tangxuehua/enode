using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        public override string GetTopic(IEvent source)
        {
            return "NoteEventTopic";
        }
    }
}
