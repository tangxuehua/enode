namespace ENode.EQueue
{
    public interface ITopicProvider<T>
    {
        string GetTopic(T source);
    }
}
