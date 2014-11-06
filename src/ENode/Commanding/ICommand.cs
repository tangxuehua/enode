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
        /// <summary>Represents the times the command should retry when the command execution has concurrent exception.
        /// </summary>
        int RetryCount { get; }
        /// <summary>Represents the extension information of the command.
        /// </summary>
        IDictionary<string, string> Items { get; }
        /// <summary>Represents a key of the command.
        /// </summary>
        /// <returns></returns>
        object GetKey();
    }
}
