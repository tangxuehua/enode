namespace ENode.Infrastructure
{
    public interface IProcessingContext
    {
        string ProcessName { get; }
        bool Process();
        bool ProcessCallback(object obj);
    }
}
