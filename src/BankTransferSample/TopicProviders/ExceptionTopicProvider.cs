using ENode.EQueue;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class ExceptionTopicProvider : AbstractTopicProvider<IPublishableException>
    {
        public override string GetTopic(IPublishableException source)
        {
            return "BankTransferExceptionTopic";
        }
    }
}
