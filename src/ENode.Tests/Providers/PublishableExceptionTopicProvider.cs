using ENode.EQueue;
using ENode.Infrastructure;

namespace ENode.Tests
{
    public class PublishableExceptionTopicProvider : AbstractTopicProvider<IPublishableException>
    {
        public override string GetTopic(IPublishableException source)
        {
            return "PublishableExceptionTopic";
        }
    }
}
