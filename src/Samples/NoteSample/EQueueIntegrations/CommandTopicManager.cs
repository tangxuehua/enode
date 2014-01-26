using ECommon.IoC;
using ENode.Commanding;
using ENode.EQueue;

namespace NoteSample.EQueueIntegrations
{
    public class CommandTopicManager : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
