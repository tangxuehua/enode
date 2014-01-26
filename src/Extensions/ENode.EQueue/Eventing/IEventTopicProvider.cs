using ENode.Eventing;

namespace ENode.EQueue
{
    public interface IEventTopicProvider
    {
        string GetTopic(EventStream eventStream);
    }
}
