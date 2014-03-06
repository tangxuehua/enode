using ENode.Commanding;
using ENode.EQueue;

namespace NoteSample.Providers
{
    public class CommandTopicProvider : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
