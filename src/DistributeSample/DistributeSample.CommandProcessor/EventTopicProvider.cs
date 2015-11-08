using System.Threading;
using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace DistributeSample.CommandProcessor
{
    [Component]
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        static int _index;
        public override string GetTopic(IDomainEvent source)
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
