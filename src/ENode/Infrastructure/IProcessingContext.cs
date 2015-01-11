namespace ENode.Infrastructure
{
    public interface IProcessingContext
    {
        string Name { get; }
        object GetHashKey();
        bool Process();
        bool Callback(object obj);
    }
}
