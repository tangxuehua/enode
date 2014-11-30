using System.Collections.Generic;

namespace ENode.EQueue
{
    public interface ITopicProvider<T>
    {
        string GetTopic(T source);
        IEnumerable<string> GetAllTopics();
    }
}
