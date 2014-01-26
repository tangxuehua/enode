using ENode.Commanding;

namespace ENode.EQueue
{
    public interface ICommandTopicProvider
    {
        string GetTopic(ICommand command);
    }
}
