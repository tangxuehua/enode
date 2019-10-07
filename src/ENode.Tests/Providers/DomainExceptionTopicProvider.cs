using ENode.Domain;
using ENode.EQueue;

namespace ENode.Tests
{
    public class DomainExceptionTopicProvider : AbstractTopicProvider<IDomainException>
    {
        public override string GetTopic(IDomainException source)
        {
            return "DomainExceptionTopic";
        }
    }
}
