using ENode.Commanding;
using ENode.EQueue;

namespace ENode.UnitTests
{
    public class CommandTopicProvider : AbstractTopicProvider<ICommand>
    {
        public override string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
