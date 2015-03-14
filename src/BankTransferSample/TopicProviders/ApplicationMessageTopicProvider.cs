using ECommon.Components;
using ENode.EQueue;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component]
    public class ApplicationMessageTopicProvider : AbstractTopicProvider<IApplicationMessage>
    {
        public override string GetTopic(IApplicationMessage applicationMessage)
        {
            return "BankTransferApplicationMessageTopic";
        }
    }
}
