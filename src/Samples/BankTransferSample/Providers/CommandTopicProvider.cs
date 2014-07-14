using ENode.Commanding;
using ENode.EQueue;

namespace BankTransferSample.Providers
{
    public class CommandTopicProvider : ICommandTopicProvider
    {
        public string GetTopic(ICommand command)
        {
            return "BankTransferCommandTopic";
        }
    }
}
