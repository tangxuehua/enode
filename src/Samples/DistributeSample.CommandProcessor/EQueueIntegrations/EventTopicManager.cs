using System.Threading;
using ENode.EQueue;
using ENode.Eventing;

namespace DistributeSample.CommandProcessor.EQueueIntegrations
{
    public class EventTopicManager : IEventTopicProvider
    {
        static int _index;
        public string GetTopic(EventStream eventStream)
        {
            if (Interlocked.Increment(ref _index) % 2 == 0)
            {
                return "NoteEventTopic2";
            }
            else
            {
                return "NoteEventTopic1";
            }
        }
    }
}
