namespace ENode.Infrastructure
{
    public interface IProcessingContext
    {
        string Name { get; }
        bool Process();
        bool Callback(object obj);
    }
}
