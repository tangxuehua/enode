using ENode.EQueue;
using ENode.Exceptions;

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
