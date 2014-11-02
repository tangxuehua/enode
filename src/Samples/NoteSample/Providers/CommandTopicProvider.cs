using ECommon.Components;
using ENode.Commanding;
using ENode.EQueue;

namespace NoteSample.Providers
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
