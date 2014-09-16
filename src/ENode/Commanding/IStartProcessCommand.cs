namespace ENode.Commanding
{
    /// <summary>Represents a command which starts a business process (saga).
    /// </summary>
    public interface IStartProcessCommand : ICommand
    {
        /// <summary>Represents the unique identifier of the business process.
        /// </summary>
        string ProcessId { get; }
    }
}
