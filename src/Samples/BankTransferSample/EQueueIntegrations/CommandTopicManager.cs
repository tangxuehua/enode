using ENode.Commanding;
using ENode.EQueue;

namespace BankTransferSample.EQueueIntegrations
{
    public class CommandTopicManager : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "BankTransferCommandTopic";
        }
    }
}
