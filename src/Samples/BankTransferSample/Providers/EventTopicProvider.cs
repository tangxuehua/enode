using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<EventStream>
    {
        public override string GetTopic(EventStream eventStream)
        {
            return "BankTransferEventTopic";
        }
    }
}
