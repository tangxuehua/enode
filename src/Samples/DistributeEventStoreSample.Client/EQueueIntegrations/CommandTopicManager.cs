using ECommon.IoC;
using ENode.Commanding;
using ENode.EQueue;

namespace DistributeEventStoreSample.Client.EQueueIntegrations
{
    public class CommandTopicManager : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "NoteCommandTopic";
        }
    }
}
