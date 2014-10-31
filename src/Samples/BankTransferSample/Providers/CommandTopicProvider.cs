using ECommon.Components;
using ENode.Commanding;
using ENode.EQueue;

namespace BankTransferSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class CommandTopicProvider : AbstractTopicProvider<ICommand>
    {
        public override string GetTopic(ICommand command)
        {
            return "BankTransferCommandTopic";
        }
    }
}
