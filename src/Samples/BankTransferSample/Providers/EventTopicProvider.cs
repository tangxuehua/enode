using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        public override string GetTopic(IEvent source)
        {
            return "BankTransferEventTopic";
        }
    }
}
