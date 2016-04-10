using ENode.EQueue;
using ENode.Infrastructure;

namespace ENode.Tests
{
    public class ApplicationMessageTopicProvider : AbstractTopicProvider<IApplicationMessage>
    {
        public override string GetTopic(IApplicationMessage source)
        {
            return "ApplicationMessageTopic";
        }
    }
}
