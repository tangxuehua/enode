using ECommon.IoC;
using ENode.Commanding;
using ENode.EQueue;

namespace NoteSample.EQueueIntegrations
{
    [Component]
    public class CommandTopicManager : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
