namespace ENode.Commanding
{
    /// <summary>Represents a command which is associated with a specific business process (saga).
    /// </summary>
    public interface IProcessCommand : ICommand
    {
        /// <summary>Represents the unique identifier of the business process.
        /// </summary>
        string ProcessId { get; set; }
    }
}
