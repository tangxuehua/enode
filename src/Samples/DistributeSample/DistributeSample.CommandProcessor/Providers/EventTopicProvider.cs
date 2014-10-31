using System.Threading;
using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace DistributeSample.CommandProcessor.Providers
{
    [Component(LifeStyle.Singleton)]
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        static int _index;
        public override string GetTopic(IEvent source)
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
