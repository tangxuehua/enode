using ENode.EQueue;
using ENode.Eventing;

namespace DistributeSample.EventProcessor.EQueueIntegrations
{
    public class EventTopicManager : IEventTopicProvider
    {
        public string GetTopic(EventStream eventStream)
        {
            return "NoteEventTopic";
        }
    }
}
