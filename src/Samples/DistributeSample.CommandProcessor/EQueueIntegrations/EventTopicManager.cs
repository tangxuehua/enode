using ENode.EQueue;
using ENode.Eventing;

namespace DistributeSample.CommandProcessor.EQueueIntegrations
{
    public class EventTopicManager : IEventTopicProvider
    {
        public string GetTopic(EventStream eventStream)
        {
            return "NoteEventTopic";
        }
    }
}
