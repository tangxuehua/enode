using ENode.Domain;
using ENode.EQueue;

namespace BankTransferSample.Providers
{
    public class ExceptionTopicProvider : AbstractTopicProvider<IDomainException>
    {
        public override string GetTopic(IDomainException source)
        {
            return Constants.ExceptionTopic;
        }
    }
}
