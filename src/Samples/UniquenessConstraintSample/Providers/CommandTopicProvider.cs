using ENode.Commanding;
using ENode.EQueue;

namespace UniquenessConstraintSample.Providers
{
    public class CommandTopicProvider : AbstractTopicProvider<ICommand>
    {
        public override string GetTopic(ICommand command)
        {
            return "SectionCommandTopic";
        }
    }
}
