using ECommon.Components;
using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class EventTopicProvider : AbstractTopicProvider<IEvent>
    {
        public override string GetTopic(IEvent source)
        {
            return "BankTransferEventTopic";
        }
    }
}
