using ENode.EQueue;
using ENode.Messaging;

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
