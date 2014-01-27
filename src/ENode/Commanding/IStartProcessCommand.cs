namespace ENode.Commanding
{
    /// <summary>Represents whether a command will start a business process.
    /// </summary>
    public interface IStartProcessCommand
    {
        object ProcessId { get; }
    }
}
