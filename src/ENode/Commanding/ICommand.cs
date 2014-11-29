using System.Collections.Generic;

namespace ENode.Commanding
{
    /// <summary>Represents a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        string Id { get; set; }
        /// <summary>Represents how many times the command should be retried when the command execution has concurrent exception.
        /// </summary>
        int RetryCount { get; }
        /// <summary>Represents a key of the command.
        /// </summary>
        /// <returns></returns>
        object GetKey();
    }
}
