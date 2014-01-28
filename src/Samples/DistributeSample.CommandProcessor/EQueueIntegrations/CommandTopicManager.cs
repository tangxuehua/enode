using ENode.Commanding;
using ENode.EQueue;

namespace DistributeSample.CommandProcessor.EQueueIntegrations
{
    public class CommandTopicManager : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
