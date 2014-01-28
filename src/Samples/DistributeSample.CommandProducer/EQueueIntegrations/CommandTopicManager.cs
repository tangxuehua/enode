using ECommon.IoC;
using ENode.Commanding;
using ENode.EQueue;

namespace DistributeSample.CommandProducer.EQueueIntegrations
{
    public class CommandTopicManager : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
