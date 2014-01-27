using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.EQueueIntegrations
{
    public class EventTopicManager : IEventTopicProvider
    {
        public string GetTopic(EventStream eventStream)
        {
            return "BankTransferEventTopic";
        }
    }
}
