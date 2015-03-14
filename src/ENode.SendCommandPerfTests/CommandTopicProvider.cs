using ECommon.Components;
using ENode.Commanding;
using ENode.EQueue;

namespace ENode.SendCommandPerfTests
{
    [Component]
    public class CommandTopicProvider : AbstractTopicProvider<ICommand>
    {
        public override string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
