namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        string Id { get; set; }
        /// <summary>Gets the target of the command.
        /// </summary>
        /// <returns></returns>
        object GetTarget();
    }
}
