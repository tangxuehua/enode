using ENode.Commanding;
using ENode.EQueue;

namespace UniquenessConstraintSample.Providers
{
    public class CommandTopicProvider : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "SectionCommandTopic";
        }
    }
}
