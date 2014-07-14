using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.Providers
{
    public class EventTopicProvider : IEventTopicProvider
    {
        public string GetTopic(EventStream eventStream)
        {
            return "BankTransferEventTopic";
        }
    }
}
