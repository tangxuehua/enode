using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.Providers
{
    public class EventTopicProvider : AbstractTopicProvider<IDomainEvent>
    {
        public override string GetTopic(IDomainEvent source)
        {
            return Constants.EventTopic;
        }
    }
}
